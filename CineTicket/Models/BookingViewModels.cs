using System.ComponentModel.DataAnnotations;

namespace CineTicket.Models
{
    public class BookingViewModel
    {
        public int MovieId { get; set; }
        public string MovieTitle { get; set; }
        public int TicketPrice { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số lượng vé.")]
        [Range(1, 10, ErrorMessage = "Bạn có thể đặt tối đa 10 vé.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        public string PaymentMethod { get; set; }
    }
}
