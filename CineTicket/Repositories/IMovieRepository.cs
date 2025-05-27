using CineTicket.Models;

namespace CineTicket.Repositories
{
    public interface IMovieRepository
    {
        List<Movie> GetAllMovies();
        Movie GetMovieById(int id);
        List<Movie> SearchMovies(string term);
        bool HasShowtime(int movieId, DateTime now);

    }
}
