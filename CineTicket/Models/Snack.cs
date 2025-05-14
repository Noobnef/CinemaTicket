using System.ComponentModel.DataAnnotations;

namespace CineTicket.Models
{
    public class Snack
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }
    }
}
