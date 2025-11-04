using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TimisStay.Data;
using TimisStay.Models;
using System.Threading.Tasks;

namespace TimisStay.Pages
{
    public class LogInModel : PageModel
    {
        private readonly TimisStayDbContext _context;

        public LogInModel(TimisStayDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Credential { get; set; }

        [BindProperty]
        public string Parola { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Clear();

            // ? 1. Validare câmpuri
            if (string.IsNullOrWhiteSpace(Credential) || string.IsNullOrWhiteSpace(Parola))
            {
                ModelState.AddModelError(string.Empty, "Please enter both your email/username and password.");
                return Page();
            }

            // ? 2. Cautã utilizatorul dupã email (sau adaugã ?i dupã username, dacã ai câmpul în DB)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == Credential);

            if (user == null)
            {
                ModelState.AddModelError("Credential", "Incorrect email or password.");
                return Page();
            }

            // ? 3. Verificã parola (hash comparare)
            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, Parola);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("Credential", "Incorrect email or password.");
                return Page();
            }

            // ? 4. Stocheazã informa?iile utilizatorului în sesiune
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
            HttpContext.Session.SetString("UserRole", user.Role);

            if (user.Role?.ToLower() == "receptionist")
            {
                return RedirectToPage("/ManageBookings");
            }
            else if (user.Role?.ToLower() == "user")
            {
                return RedirectToPage("/BookNow");
            }

            // fallback în caz cã rolul e necunoscut
            return RedirectToPage("/Index");
        }
    }
}
