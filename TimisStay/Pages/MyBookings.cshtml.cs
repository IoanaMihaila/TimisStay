using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TimisStay.Data;
using TimisStay.Models;
using TimisStay.Services;

namespace TimisStay.Pages
{
    public class MyBookingsModel : PageModel
    {
        private readonly TimisStayDbContext _context;

        private readonly IEmailService _emailService;

        public MyBookingsModel(TimisStayDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }


        public List<Booking> MyBookings { get; set; } = new();

        public List<Booking> PastBookings { get; set; } = new();

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to view your bookings.";
                return RedirectToPage("/Login");
            }

            var bookings = _context.UserBookings
                .Where(ub => ub.UserId == userId)
                .Include(ub => ub.Booking)
                    .ThenInclude(b => b.RoomBookings)
                        .ThenInclude(rb => rb.Room)
                .Select(ub => ub.Booking)
                .OrderByDescending(b => b.CheckInDate)
                .ToList();

            MyBookings = bookings;
            PastBookings = bookings.Where(b => b.CheckOutDate < DateTime.Now).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAddReviewAsync(int Rating, string Comment)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to leave a review.";
                return RedirectToPage("/Login");
            }

            var review = new Review
            {
                UserId = userId.Value,
                Rating = Rating,
                Comment = Comment
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["ReviewSuccess"] = "True";
            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostCancelBookingAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in.";
                return RedirectToPage("/Login");
            }

            var booking = await _context.Bookings
                .Include(b => b.UserBookings)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToPage();
            }

            // ?? Verificãm cã rezervarea apar?ine utilizatorului curent
            if (!booking.UserBookings.Any(ub => ub.UserId == userId))
            {
                TempData["ErrorMessage"] = "You cannot cancel this booking.";
                return RedirectToPage();
            }

            // ?? Schimbãm statusul în "Canceled"
            booking.Status = "Canceled";

            await _context.SaveChangesAsync();

            TempData["CancelSuccess"] = "True";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditBookingAsync(int BookingId, DateTime CheckInDate, DateTime CheckOutDate, int NrAdults, int NrChildren)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in.";
                return RedirectToPage("/Login");
            }

            var booking = await _context.Bookings
                .Include(b => b.UserBookings)
                .Include(b => b.RoomBookings)
                .ThenInclude(rb => rb.Room)
                .FirstOrDefaultAsync(b => b.BookingId == BookingId);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToPage();
            }

            // Verificãm dacã rezervarea apar?ine utilizatorului
            if (!booking.UserBookings.Any(ub => ub.UserId == userId))
            {
                TempData["ErrorMessage"] = "You cannot edit this booking.";
                return RedirectToPage();
            }

            var roomBooking = booking.RoomBookings.FirstOrDefault();
            var room = roomBooking?.Room;

            if (room == null)
            {
                TempData["ErrorMessage"] = "Room not found.";
                return RedirectToPage();
            }

            // Validare numãr de adul?i/copii
            if (NrAdults > room.MaxAdults || NrChildren > room.MaxChildren)
            {
                TempData["ErrorMessage"] = $"This room allows maximum {room.MaxAdults} adults and {room.MaxChildren} children.";
                return RedirectToPage();
            }

            // Verificãm disponibilitatea camerei pentru noile date
            var isAvailable = !await _context.RoomBookings.AnyAsync(rb =>
                rb.RoomId == room.RoomId &&
                rb.BookingId != BookingId && // Exclude rezervarea curentã
                (
                    (CheckInDate >= rb.Booking.CheckInDate && CheckInDate < rb.Booking.CheckOutDate) ||
                    (CheckOutDate > rb.Booking.CheckInDate && CheckOutDate <= rb.Booking.CheckOutDate) ||
                    (CheckInDate <= rb.Booking.CheckInDate && CheckOutDate >= rb.Booking.CheckOutDate)
                )
            );

            if (!isAvailable)
            {
                TempData["ErrorMessage"] = "The room is not available for the selected dates.";
                return RedirectToPage();
            }

            // Actualizare rezervare
            booking.CheckInDate = CheckInDate;
            booking.CheckOutDate = CheckOutDate;
            booking.NrAdults = NrAdults;
            booking.NrChildren = NrChildren;

            // Recalculare pre? total
            var nights = (CheckOutDate - CheckInDate).TotalDays;
            booking.TotalPrice = room.PricePerNight * (decimal)nights;

            await _context.SaveChangesAsync();

            TempData["EditSuccess"] = "True";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSimulatePaymentAsync(int BookingId, string CardNumber, string ExpiryDate, string CVV)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to make a payment.";
                return RedirectToPage("/Login");
            }

            var booking = await _context.Bookings
                .Include(b => b.UserBookings)
                .ThenInclude(ub => ub.User) 
                .FirstOrDefaultAsync(b => b.BookingId == BookingId);


            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToPage();
            }

            if (!booking.UserBookings.Any(ub => ub.UserId == userId))
            {
                TempData["ErrorMessage"] = "You cannot pay for this booking.";
                return RedirectToPage();
            }

            if (booking.Status == "Paid")
            {
                TempData["ErrorMessage"] = "This booking is already paid.";
                return RedirectToPage();
            }

            // ?? Simulare platã (po?i adãuga validãri de bazã)
            bool validCard = CardNumber.Replace(" ", "").Length == 16 && CVV.Length == 3;
            if (!validCard)
            {
                TempData["ErrorMessage"] = "Invalid card details. Please try again.";
                return RedirectToPage();
            }

            // ?? Actualizare status rezervare
            booking.Status = "Paid";
            await _context.SaveChangesAsync();

            var user = booking.UserBookings.FirstOrDefault()?.User;

            if (user != null)
            {
                var subject = "Your Booking Has Been Paid - TimisStay";
                var body = $@"
                    <h2>Dear {user.FirstName},</h2>
                    <p>Your booking for <strong>{booking.RoomType}</strong> 
                    from <strong>{booking.CheckInDate:dd MMM yyyy}</strong> 
                    to <strong>{booking.CheckOutDate:dd MMM yyyy}</strong> 
                    has been <span style='color:blue;font-weight:bold;'>paid</span>.</p>
                    <p>Thank you for your payment! We are waiting for you!</p>
                    <br><p>Best regards,<br>TimisStay Team</p>";
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }

            TempData["PaymentSuccess"] = "True";
            return RedirectToPage();
        }

    }
}
