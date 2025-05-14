namespace CineTicket.Models
{
    public class SnackOrder
    {
        public int Id { get; set; }
        public int SnackId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime BookingTime { get; set; }
        public string? UserId { get; set; }
        public int ShowtimeId { get; set; }

        public Snack Snack { get; set; }
    }
}
