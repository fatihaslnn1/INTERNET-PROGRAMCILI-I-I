using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proje2.Data;
using Proje2.Models;

namespace Proje2.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var movies = await _db.Movies
            .Include(m => m.Comments.Where(c => c.IsApproved))
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
        return View(movies);
    }

    public async Task<IActionResult> Movie(int id)
    {
        var movie = await _db.Movies
            .Include(m => m.Comments.Where(c => c.IsApproved))
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null)
            return NotFound();

        return View(movie);
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(int movieId, string authorName, string content)
    {
        if (string.IsNullOrWhiteSpace(authorName) || string.IsNullOrWhiteSpace(content))
        {
            TempData["Error"] = "İsim ve yorum alanları boş bırakılamaz.";
            return RedirectToAction(nameof(Movie), new { id = movieId });
        }

        var comment = new Comment
        {
            MovieId = movieId,
            AuthorName = authorName.Trim(),
            Content = content.Trim(),
            IsApproved = false,
            CreatedAt = DateTime.Now
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Yorumunuz alındı. Admin onayından sonra yayınlanacak.";
        return RedirectToAction(nameof(Movie), new { id = movieId });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
