using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CineTicket.Models;
using CineTicket.Repositories;

namespace CineTicket.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMovieRepository _movieRepository;

        public HomeController(ILogger<HomeController> logger, IMovieRepository movieRepository)
        {
            _logger = logger;
            _movieRepository = movieRepository;
        }

        public IActionResult Index()
        {
            var movies = _movieRepository.GetAllMovies()
                .OrderByDescending(m => m.ReleaseDate)
                .ToList();
            return View(movies);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
