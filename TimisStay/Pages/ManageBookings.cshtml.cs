using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TimisStay.Models;
using TimisStay.Data;
using TimisStay.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TimisStay.Pages
{
    public class ManageBookingsModel : PageModel
    {
        private readonly TimisStayDbContext _context;
        private readonly IEmailService _emailService;

        public ManageBookingsModel(TimisStayDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public List<Booking> Bookings { get; set; } = new();

        public async Task OnGetAsync()
        {
            Bookings = await _context.Bookings
                .Include(b => b.RoomBookings)
                    .ThenInclude(rb => rb.Room)
                .Include(b => b.UserBookings)
                    .ThenInclude(ub => ub.User)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostConfirmAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.UserBookings)
                    .ThenInclude(ub => ub.User)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound();

            booking.Status = "Confirmed";
            await _context.SaveChangesAsync();

            var user = booking.UserBookings.FirstOrDefault()?.User;
            if (user != null)
            {
                var subject = "Your Booking Has Been Confirmed - Timi?Stay";
                var body = $@"
                    <h2>Dear {user.FirstName},</h2>
                    <p>Your booking for <strong>{booking.RoomType}</strong> 
                    from <strong>{booking.CheckInDate:dd MMM yyyy}</strong> 
                    to <strong>{booking.CheckOutDate:dd MMM yyyy}</strong> 
                    has been <span style='color:green;font-weight:bold;'>confirmed</span>.</p>
                    <p>We look forward to welcoming you!</p>
                    <br><p>Best regards,<br>Timi?Stay Team</p>";
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.UserBookings)
                    .ThenInclude(ub => ub.User)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound();

            //booking.Status = "Canceled";
            //await _context.SaveChangesAsync();

            var user = booking.UserBookings.FirstOrDefault()?.User;

            var roomType = booking.RoomType;
            var checkIn = booking.CheckInDate;
            var checkOut = booking.CheckOutDate;

            if (booking.RoomBookings != null && booking.RoomBookings.Any())
                _context.RoomBookings.RemoveRange(booking.RoomBookings);

            if (booking.UserBookings != null && booking.UserBookings.Any())
                _context.UserBookings.RemoveRange(booking.UserBookings);

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            if (user != null)
            {
                var subject = "Your Booking Has Been Deleted - TimisStay";
                var body = $@"
                    <h2>Dear {user.FirstName},</h2>
                    <p>Your booking for <strong>{booking.RoomType}</strong> 
                    from <strong>{booking.CheckInDate:dd MMM yyyy}</strong> 
                    to <strong>{booking.CheckOutDate:dd MMM yyyy}</strong> 
                    has been <span style='color:red;font-weight:bold;'>deleted</span>.</p>
                    <p>If you have questions, please contact us.</p>
                    <br><p>Best regards,<br>TimisStay Team</p>";
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }

            return RedirectToPage();
        }
    }
}
