using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TimisStay.Data;
using TimisStay.Models;
using System.Collections.Generic;
using System.Linq;

public class IndexModel : PageModel
{
    private readonly TimisStayDbContext _context;

    public IndexModel(TimisStayDbContext context)
    {
        _context = context;
    }

    public List<Review> Reviews { get; set; } = new();

    public void OnGet()
    {
        Reviews = _context.Reviews
            .Include(r => r.User)
            .OrderByDescending(r => r.ReviewId)
            .Take(6)
            .ToList();
    }
}
