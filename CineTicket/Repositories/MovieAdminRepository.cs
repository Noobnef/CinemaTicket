using CineTicket.Models;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Repositories
{
    public class MovieAdminRepository : IMovieAdminRepository
    {
        private readonly ApplicationDbContext _context;
        public MovieAdminRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Movie>> GetAllMoviesAsync()
        {
            return await _context.Movies.ToListAsync();
        }

        public async Task<Movie> GetMovieByIdAsync(int id)
        {
            return await _context.Movies.FindAsync(id);
        }

        public async Task AddMovieAsync(Movie movie)
        {
            await _context.Movies.AddAsync(movie);
        }

        public async Task UpdateMovieAsync(Movie movie)
        {
            _context.Movies.Update(movie);
            await Task.CompletedTask;
        }

        public async Task DeleteMovieAsync(Movie movie)
        {
            _context.Movies.Remove(movie);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
