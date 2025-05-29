using CineTicket.Models;

namespace CineTicket.Repositories
{
    public interface IBookingRepository
    {
        Movie GetMovie(int movieId);
        List<Showtime> GetShowtimes(int movieId);
        Showtime GetShowtimeWithRoom(int showtimeId, int movieId);
        List<string> GetBookedSeats(int showtimeId);
        List<Snack> GetSnacks();
        Task AddTicketsAsync(IEnumerable<Ticket> tickets);
        Task AddSnackOrdersAsync(IEnumerable<SnackOrder> snackOrders);
        Task AddBookingHistoryAsync(BookingHistory history);
        Task SaveChangesAsync();
        Task<Snack> FindSnackAsync(int snackId);
        Task<Movie> FindMovieAsync(int movieId);
        Task<BookingHistory> GetLatestBookingHistoryAsync(string userId);
        Task<List<string>> GetSnackNamesForHistoryAsync(string userId, int showtimeId, DateTime bookingDate);
        Task UpdateBookingHistoryPaidAsync(string orderId);
        Task<BookingHistory> GetLatestUnpaidBookingHistoryAsync(string userId);
        Task<BookingHistory> GetBookingHistoryByIdAsync(int id, string userId);
        Task<BookingHistory> GetBookingHistoryByOrderIdAsync(string orderId, string userId);

    }
}
