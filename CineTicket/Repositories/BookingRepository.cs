using CineTicket.Models;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;
        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Movie GetMovie(int movieId)
        {
            return _context.Movies.FirstOrDefault(m => m.Id == movieId);
        }

        public List<Showtime> GetShowtimes(int movieId)
        {
            return _context.Showtimes
                .Where(s => s.MovieId == movieId && s.StartTime > DateTime.Now)
                .OrderBy(s => s.StartTime)
                .ToList();
        }

        public Showtime GetShowtimeWithRoom(int showtimeId, int movieId)
        {
            return _context.Showtimes
                .Include(s => s.Room)
                .FirstOrDefault(s => s.Id == showtimeId && s.MovieId == movieId);
        }

        public List<string> GetBookedSeats(int showtimeId)
        {
            return _context.Tickets
                .Where(t => t.ShowtimeId == showtimeId)
                .Select(t => t.SeatNumber)
                .ToList();
        }

        public List<Snack> GetSnacks()
        {
            return _context.Snacks.ToList();
        }

        public async Task AddTicketsAsync(IEnumerable<Ticket> tickets)
        {
            await _context.Tickets.AddRangeAsync(tickets);
        }

        public async Task AddSnackOrdersAsync(IEnumerable<SnackOrder> snackOrders)
        {
            await _context.SnackOrder.AddRangeAsync(snackOrders);
        }

        public async Task AddBookingHistoryAsync(BookingHistory history)
        {
            await _context.BookingHistories.AddAsync(history);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Snack> FindSnackAsync(int snackId)
        {
            return await _context.Snacks.FindAsync(snackId);
        }

        public async Task<Movie> FindMovieAsync(int movieId)
        {
            return await _context.Movies.FindAsync(movieId);
        }

        public async Task<BookingHistory> GetLatestBookingHistoryAsync(string userId)
        {
            return await _context.BookingHistories
                .Where(h => h.UserId == userId)
                .Include(h => h.Showtime)
                    .ThenInclude(s => s.Movie)
                .OrderByDescending(h => h.BookingDate)
                .FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetSnackNamesForHistoryAsync(string userId, int showtimeId, DateTime bookingDate)
        {
            return await _context.SnackOrder
                .Where(s => s.UserId == userId
                            && s.ShowtimeId == showtimeId
                            && EF.Functions.DateDiffSecond(s.BookingTime, bookingDate) <= 5)
                .Include(s => s.Snack)
                .Select(s => s.Snack.Name)
                .ToListAsync();
        }
    }
}
