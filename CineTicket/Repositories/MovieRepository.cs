using CineTicket.Models;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly ApplicationDbContext _context;
        public MovieRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Movie> GetAllMovies()
        {
            return _context.Movies.ToList();
        }

        public Movie GetMovieById(int id)
        {
            return _context.Movies.FirstOrDefault(m => m.Id == id);
        }

        public List<Movie> SearchMovies(string term)
        {
            return _context.Movies
                .Where(m => EF.Functions.Like(m.Title, $"%{term}%"))
                .ToList();
        }

        public bool HasShowtime(int movieId, DateTime now)
        {
            return _context.Showtimes.Any(s => s.MovieId == movieId && s.StartTime > now);
        }
    }

}
