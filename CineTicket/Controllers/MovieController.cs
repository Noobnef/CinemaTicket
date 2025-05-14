using CineTicket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Areas.Admin.Controllers;
[Authorize]

public class MovieController : Controller

    {

        private readonly ApplicationDbContext _context;
        public MovieController(ApplicationDbContext context)
        {
            _context = context;
        }

    public IActionResult Index()
    {
        var now = DateTime.Now;

        var movies = _context.Movies
            .Select(m => new Movie
            {
                Id = m.Id,
                Title = m.Title,
                Duration = m.Duration,
                PosterUrl = m.PosterUrl,
                BannerUrl = m.BannerUrl,
                HasShowtime = _context.Showtimes.Any(s => s.MovieId == m.Id && s.StartTime > now)
            })
            .ToList();

        return View(movies);
    }

    public IActionResult Details(int id)
        {
            var movie = _context.Movies.FirstOrDefault(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }
        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var results = await _context.Movies
                .Where(m => EF.Functions.Like(m.Title, $"%{term}%"))
                .Select(m => new
                {
                    label = m.Title,   
                    value = m.Id
                })
                .Take(10)
                .ToListAsync();

            return Json(results);
        }
    }
