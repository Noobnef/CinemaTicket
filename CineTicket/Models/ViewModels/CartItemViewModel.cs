namespace CineTicket.ViewModels
{
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int BookingId { get; set; } // Thêm thuộc tính này vào ViewModel

        public string MovieTitle { get; set; }
        public DateTime Showtime { get; set; }
        public string SeatNumbers { get; set; }
        public List<string> SnackNames { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime BookingDate { get; set; }
        public bool IsPaid { get; set; }
    }
}
