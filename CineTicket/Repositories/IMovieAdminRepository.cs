using CineTicket.Models;

namespace CineTicket.Repositories
{
    public interface IMovieAdminRepository
    {
        Task<List<Movie>> GetAllMoviesAsync();
        Task<Movie> GetMovieByIdAsync(int id);
        Task AddMovieAsync(Movie movie);
        Task UpdateMovieAsync(Movie movie);
        Task DeleteMovieAsync(Movie movie);
        Task SaveChangesAsync();
    }
}
