using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimisStay.Data;
using TimisStay.Models;

namespace TimisStay.Pages
{
    public class GalleryModel : PageModel
    {
        private readonly TimisStayDbContext _context;
        private readonly IWebHostEnvironment _env;

        public GalleryModel(TimisStayDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public List<RoomPhoto> Photos { get; set; }

        public async Task OnGetAsync()
        {
            Photos = await _context.RoomPhotos.ToListAsync();
        }
        [BindProperty]
        public IFormFile PhotoFile { get; set; }

        public async Task<IActionResult> OnPostUploadPhotoAsync()
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";
            if (userRole != "Receptionist")
                return Forbid(); // doar receptionists pot urca poze

            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                // creeazã folderul uploads dacã nu existã
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Path.GetFileNameWithoutExtension(PhotoFile.FileName)}_{System.Guid.NewGuid()}{Path.GetExtension(PhotoFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await PhotoFile.CopyToAsync(fileStream);
                }

                var photo = new RoomPhoto
                {
                    PhotoPath = $"/uploads/{uniqueFileName}"
                };

                _context.RoomPhotos.Add(photo);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
