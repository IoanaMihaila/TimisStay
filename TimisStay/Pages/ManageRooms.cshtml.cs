using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TimisStay.Data;
using TimisStay.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TimisStay.Pages
{
    public class ManageRoomsModel : PageModel
    {
        private readonly TimisStayDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ManageRoomsModel(TimisStayDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public List<Room> Rooms { get; set; } = new();

        public async Task OnGetAsync()
        {
            Rooms = await _context.Rooms
                .Include(r => r.RoomDetailsPhotos)
                .ToListAsync();
        }

        [BindProperty]
        public int RoomId { get; set; }
        [BindProperty]
        public string RoomType { get; set; }
        [BindProperty]
        public string? Description { get; set; }
        [BindProperty]
        public decimal PricePerNight { get; set; }
        [BindProperty]
        public int MaxAdults { get; set; }
        [BindProperty]
        public int MaxChildren { get; set; }
        [BindProperty]
        public bool IsAvailable { get; set; }
        [BindProperty]
        public string? PhotoCategory { get; set; }

        public async Task<IActionResult> OnPostEditRoomAsync()
        {
            var room = await _context.Rooms.FindAsync(RoomId);
            if (room == null)
                return NotFound();

            room.RoomType = RoomType;
            room.Description = Description;
            room.PricePerNight = PricePerNight;
            room.MaxAdults = MaxAdults;
            room.MaxChildren = MaxChildren;
            room.IsAvailable = IsAvailable;

            var categories = new[] { "Bathroom", "Living", "Kitchen", "Other" };

            foreach (var category in categories)
            {
                var photoFile = Request.Form.Files[$"DetailPhotos_{category}"];
                if (photoFile != null && photoFile.Length > 0)
                {
                    var existingPhoto = await _context.RoomDetailsPhotos
                        .FirstOrDefaultAsync(p => p.RoomId == room.RoomId && p.Category == category);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photoFile.FileName)}";
                    var uploadPath = Path.Combine(_env.WebRootPath, "images", "roomdetails");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await photoFile.CopyToAsync(stream);

                    if (existingPhoto != null)
                    {
                        // ?terge poza veche
                        var oldFilePath = Path.Combine(_env.WebRootPath, existingPhoto.PhotoPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);

                        existingPhoto.PhotoPath = $"/images/roomdetails/{fileName}";
                    }
                    else
                    {
                        _context.RoomDetailsPhotos.Add(new RoomDetailsPhoto
                        {
                            RoomId = room.RoomId,
                            Category = category,
                            PhotoPath = $"/images/roomdetails/{fileName}"
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
