using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TimisStay.Data;
using TimisStay.Models;
using System.Threading.Tasks;

namespace TimisStay.Pages
{
    public class RoomDetailsModel : PageModel
    {
        private readonly TimisStayDbContext _context;

        public RoomDetailsModel(TimisStayDbContext context)
        {
            _context = context;
        }

        public Room Room { get; set; }


        [BindProperty(SupportsGet = true)]
        public DateTime? CheckIn { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? CheckOut { get; set; }

        [BindProperty(SupportsGet = true)]
        public int NrAdults { get; set; }

        [BindProperty(SupportsGet = true)]
        public int NrChildren { get; set; }

        public async Task<IActionResult> OnGetAsync(int roomId)
        {
            // Cautã camera în baza de date
            Room = await _context.Rooms
        .Include(r => r.RoomDetailsPhotos)
        .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (Room == null)
            {
                TempData["ErrorMessage"] = "Room not found.";
                return RedirectToPage("/BookNow");
            }

            return Page();
        }
        public bool BookingSuccess { get; set; }

        public async Task<IActionResult> OnPostBookAsync(int roomId, int nrAdults, int nrChildren, DateTime checkIn, DateTime checkOut)
        {
            var today = DateTime.Today;

            if (checkIn < today || checkOut <= today)
            {
                TempData["BookingError"] = "You cannot make a booking for past dates.";
                return RedirectToPage(); // rãmâne pe RoomDetails
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
            {
                TempData["BookingError"] = "Room not found.";
                return RedirectToPage();
            }

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

            _context.RoomBookings.Add(new RoomBooking { BookingId = booking.BookingId, RoomId = room.RoomId, Price = room.PricePerNight });
            _context.UserBookings.Add(new UserBooking { UserId = userId, BookingId = booking.BookingId });

            await _context.SaveChangesAsync();

            TempData["BookingSuccess"] = true; // marcheazã booking-ul ca succes
            return RedirectToPage(); // redirect la aceea?i paginã RoomDetails
        }


    }
}
