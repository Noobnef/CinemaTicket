using CineTicket.Models;
using CineTicket.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineTicket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class MoviesController : Controller
    {
        private readonly IMovieAdminRepository _movieRepo;

        public MoviesController(IMovieAdminRepository movieRepo)
        {
            _movieRepo = movieRepo;
        }

        public async Task<IActionResult> Index()
        {
            var movies = await _movieRepo.GetAllMoviesAsync();
            return View(movies);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Movie movie)
        {
            if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState)
                {
                    foreach (var error in kvp.Value.Errors)
                    {
                        Console.WriteLine($"Field: {kvp.Key}, Error: {error.ErrorMessage}");
                    }
                }
                return View(movie);
            }

            await _movieRepo.AddMovieAsync(movie);
            await _movieRepo.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _movieRepo.GetMovieByIdAsync(id);
            if (movie == null)
                return NotFound();
            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie)
        {
            if (id != movie.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                await _movieRepo.UpdateMovieAsync(movie);
                await _movieRepo.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _movieRepo.GetMovieByIdAsync(id);
            if (movie == null)
                return NotFound();

            return View(movie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _movieRepo.GetMovieByIdAsync(id);
            if (movie != null)
            {
                await _movieRepo.DeleteMovieAsync(movie);
                await _movieRepo.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
