using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TimisStay.Data;      // contextul bazei de date
using TimisStay.Models;    // modelul User
using System.Threading.Tasks;
using System.Linq;

namespace TimisStay.Pages
{
    public class SignInModel : PageModel
    {
        private readonly TimisStayDbContext _context; // înlocuie?te cu numele real al contextului tãu (ex: TimisStayDbContext)
        private readonly PasswordHasher<User> _passwordHasher;

        public SignInModel(TimisStayDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        [BindProperty]
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone number must have 10 digits.")]
        public string Phone { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must have at least 6 characters.")]
        public string Password { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 1?? Validare generalã
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 2?? Verificã dacã emailul existã deja
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return Page();
            }

            // 3?? Creeazã un nou utilizator
            var user = new User
            {
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                Email = Email.Trim(),
                Phone = Phone.Trim(),
                Role = "User"
            };

            // 4?? Hash parola înainte de salvare
            user.PasswordHash = _passwordHasher.HashPassword(user, Password);

            // 5?? Salveazã în baza de date
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 6?? Mesaj de succes (sau redirec?ionare)
            TempData["SuccessMessage"] = "Your account has been created successfully! You can now log in.";

            return RedirectToPage("/LogIn");
        }
    }
}
