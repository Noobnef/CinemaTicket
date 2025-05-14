using CineTicket.Models;
using System.ComponentModel.DataAnnotations;

namespace CineTicket.Models
{
    public class Poster
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Title { get; set; }
        
        [Required]
        public string Banner { get; set; }
        
        [Required]
        public string Description { get; set; }
    }
}
