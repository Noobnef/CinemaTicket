using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CineTicket.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SeatCount { get; set; }

        public int TicketPrice { get; set; }
        [ValidateNever]

        public ICollection<Seat> Seats { get; set; }
        [ValidateNever]

        public ICollection<Showtime> Showtimes { get; set; }
    }
}
