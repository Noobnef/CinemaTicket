namespace CineTicket.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SeatCount { get; set; }

        public int TicketPrice { get; set; }

        public ICollection<Seat> Seats { get; set; }
        public ICollection<Showtime> Showtimes { get; set; }
    }
}
