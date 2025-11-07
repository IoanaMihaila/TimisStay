using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimisStay.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TimisStay.Pages
{
    public class ContactModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<ContactModel> _logger;

        [BindProperty]
        public string Subject { get; set; } = string.Empty;

        [BindProperty]
        public string Message { get; set; } = string.Empty;

        public ContactModel(IEmailService emailService, ILogger<ContactModel> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var userName = HttpContext.Session.GetString("UserName") ?? "Guest";
                var userEmail = HttpContext.Session.GetString("UserEmail") ?? "N/A";

                string emailBody = $@"
                    <h3>New Contact Message from {userName}</h3>
                    <p><strong>Email:</strong> {userEmail}</p>
                    <p><strong>Subject:</strong> {Subject}</p>
                    <p><strong>Message:</strong></p>
                    <p>{Message}</p>
                ";

                await _emailService.SendEmailAsync(
                    "ioanamihaila30@yahoo.com", 
                    $"[TimisStay Contact] {Subject}",
                    emailBody
                );

                TempData["ContactSuccess"] = "True";
                return RedirectToPage();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact message");
                TempData["ContactError"] = "There was an issue sending your message. Please try again later.";
                return RedirectToPage();
            }
        }
    }
}
