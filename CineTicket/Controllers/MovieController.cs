using CineTicket.Models;
using CineTicket.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineTicket.Areas.Admin.Controllers
{
    public class MovieController : Controller
    {
        private readonly IMovieRepository _movieRepository;
        public MovieController(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public IActionResult Index()
        {
            var now = DateTime.Now;
            var movies = _movieRepository.GetAllMovies()
                .Select(m => new Movie
                {
                    Id = m.Id,
                    Title = m.Title,
                    Duration = m.Duration,
                    PosterUrl = m.PosterUrl,
                    BannerUrl = m.BannerUrl,
                    HasShowtime = _movieRepository.HasShowtime(m.Id, now)
                })
                .ToList();

            return View(movies);
        }

        public IActionResult Details(int id)
        {
            var movie = _movieRepository.GetMovieById(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        [HttpGet]
        public IActionResult Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var results = _movieRepository.SearchMovies(term)
                .Select(m => new
                {
                    label = m.Title,
                    value = m.Id
                })
                .Take(10)
                .ToList();

            return Json(results);
        }
    }
}
