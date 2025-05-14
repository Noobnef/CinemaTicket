namespace CineTicket.ViewModels
{
    public class CartItemViewModel
    {
        public string MovieTitle { get; set; }
        public DateTime Showtime { get; set; }
        public string SeatNumbers { get; set; }
        public List<string> SnackNames { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime BookingDate { get; set; }
    }
}
