using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TimisStay.Data;
using TimisStay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimisStay.Pages
{
    public class BookNowModel : PageModel
    {
        private readonly TimisStayDbContext _context;

        public BookNowModel(TimisStayDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DateTime CheckInDate { get; set; }

        [BindProperty]
        public DateTime CheckOutDate { get; set; }

        [BindProperty]
        public int NrAdults { get; set; }

        [BindProperty]
        public int NrChildren { get; set; }

        [BindProperty]
        public string RoomType { get; set; }

        [BindProperty]
        public decimal TotalPrice { get; set; }

        [TempData]
        public bool BookingSuccess { get; set; } = false;

        public List<Room> AvailableRooms { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var today = DateTime.Today;

            // ?? Validare: fãrã rezervãri în trecut
            if (CheckInDate < today || CheckOutDate <= today)
            {
                TempData["BookingError"] = "You cannot make a booking for past dates.";
                return RedirectToPage(); // redirect, nu return Page()
            }

            // ?? Validare: interval valid
            if (CheckInDate == default || CheckOutDate == default || CheckInDate >= CheckOutDate)
            {
                TempData["BookingError"] = "Please select valid check-in and check-out dates.";
                return RedirectToPage();
            }

            if (NrAdults <= 0)
            {
                TempData["BookingError"] = "Please specify at least one adult.";
                return RedirectToPage();
            }

            // ?? Selecteazã camerele disponibile doar dacã datele sunt valide
            AvailableRooms = await _context.Rooms
                .Where(r =>
                    r.IsAvailable == true &&
                    r.MaxAdults >= NrAdults &&
                    r.MaxChildren >= NrChildren &&
                    !_context.RoomBookings.Any(rb =>
                        rb.RoomId == r.RoomId &&
                        (
                            (CheckInDate >= rb.Booking.CheckInDate && CheckInDate < rb.Booking.CheckOutDate) ||
                            (CheckOutDate > rb.Booking.CheckInDate && CheckOutDate <= rb.Booking.CheckOutDate) ||
                            (CheckInDate <= rb.Booking.CheckInDate && CheckOutDate >= rb.Booking.CheckOutDate)
                        )
                    )
                )
                .ToListAsync();

            return Page();
        }


        // Metoda pentru a salva booking-ul când user-ul dã click pe Book Now
        public async Task<IActionResult> OnPostBookAsync(int roomId, int nrAdults, int nrChildren, DateTime checkIn, DateTime checkOut)
        {
            var today = DateTime.Today;

            // ?? Validare: fãrã rezervãri în trecut
            if (checkIn < today || checkOut <= today)
            {
                TempData["BookingError"] = "You cannot make a booking for past dates.";
                return RedirectToPage();
            }

            if (checkIn >= checkOut)
            {
                TempData["BookingError"] = "Check-out date must be after check-in date.";
                return RedirectToPage();
            }

            var userIdObj = HttpContext.Session.GetInt32("UserId");
            if (userIdObj == null)
                return RedirectToPage("/Account/Login");

            int userId = userIdObj.Value;

            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
                return NotFound();

            // Creeazã booking folosind datele din search
            var booking = new Booking
            {
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                NrAdults = nrAdults,
                NrChildren = nrChildren,
                RoomType = room.RoomType,
                TotalPrice = room.PricePerNight * (decimal)(checkOut - checkIn).TotalDays,
                Status = "Pending"
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var roomBooking = new RoomBooking
            {
                BookingId = booking.BookingId,
                RoomId = room.RoomId,
                Price = room.PricePerNight
            };
            _context.RoomBookings.Add(roomBooking);

            var userBooking = new UserBooking
            {
                UserId = userId,
                BookingId = booking.BookingId
            };
            _context.UserBookings.Add(userBooking);

            await _context.SaveChangesAsync();

            BookingSuccess = true;
            return RedirectToPage();
        }
    }
}
