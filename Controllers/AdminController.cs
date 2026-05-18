using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proje2.Data;
using Proje2.Models;

namespace Proje2.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AdminController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.MovieCount = await _db.Movies.CountAsync();
        ViewBag.PendingCount = await _db.Comments.CountAsync(c => !c.IsApproved);
        ViewBag.ApprovedCount = await _db.Comments.CountAsync(c => c.IsApproved);
        return View();
    }

    public async Task<IActionResult> Movies()
    {
        var movies = await _db.Movies.OrderByDescending(m => m.CreatedAt).ToListAsync();
        return View(movies);
    }

    [HttpGet]
    public IActionResult AddMovie() => View();

    [HttpPost]
    public async Task<IActionResult> AddMovie(Movie movie, IFormFile? photo)
    {
        if (!ModelState.IsValid)
            return View(movie);

        if (photo != null && photo.Length > 0)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "images", "movies");
            Directory.CreateDirectory(uploadsDir);
            var fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);
            var filePath = Path.Combine(uploadsDir, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(stream);
            movie.PhotoPath = "/images/movies/" + fileName;
        }

        movie.CreatedAt = DateTime.Now;
        _db.Movies.Add(movie);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Film başarıyla eklendi.";
        return RedirectToAction(nameof(Movies));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMovie(int id)
    {
        var movie = await _db.Movies.FindAsync(id);
        if (movie != null)
        {
            if (!string.IsNullOrEmpty(movie.PhotoPath))
            {
                var fullPath = Path.Combine(_env.WebRootPath, movie.PhotoPath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }
            _db.Movies.Remove(movie);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Movies));
    }

    public async Task<IActionResult> Comments()
    {
        var comments = await _db.Comments
            .Include(c => c.Movie)
            .OrderBy(c => c.IsApproved)
            .ThenByDescending(c => c.CreatedAt)
            .ToListAsync();
        return View(comments);
    }

    [HttpPost]
    public async Task<IActionResult> ApproveComment(int id)
    {
        var comment = await _db.Comments.FindAsync(id);
        if (comment != null)
        {
            comment.IsApproved = true;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Comments));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var comment = await _db.Comments.FindAsync(id);
        if (comment != null)
        {
            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Comments));
    }
}
