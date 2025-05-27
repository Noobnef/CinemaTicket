using CineTicket.Models;

namespace CineTicket.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _context;
        public TicketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Showtime GetShowtime(int showtimeId)
        {
            return _context.Showtimes.FirstOrDefault(s => s.Id == showtimeId);
        }

        public void AddTicket(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

    }
}
