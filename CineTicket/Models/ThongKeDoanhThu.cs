namespace CineTicket.Models
{
    public class ThongKeDoanhThu
    {
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TicketsSold { get; set; }
        public decimal SnackRevenue { get; set; }
        public int SnacksSold { get; set; }
    }

}
