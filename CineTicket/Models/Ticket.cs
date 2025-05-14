using CineTicket.Models;


namespace CineTicket.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public DateTime BookingTime { get; set; } = DateTime.Now;


        public int ShowtimeId { get; set; }
        public Showtime Showtimes { get; set; }


        public string SeatNumber { get; set; }
        public int Price { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}