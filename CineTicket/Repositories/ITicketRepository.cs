using CineTicket.Models;

namespace CineTicket.Repositories
{
    public interface ITicketRepository
    {
        Showtime GetShowtime(int showtimeId);
        void AddTicket(Ticket ticket);
        void SaveChanges();

    }
}
