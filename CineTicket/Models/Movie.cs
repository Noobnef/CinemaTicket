using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CineTicket.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Genre { get; set; }

        [Required]
        public DateTime ReleaseDate { get; set; }

        public string PosterUrl { get; set; }

        public string BannerUrl { get; set; }
        public bool HasShowtime { get; set; } 

        public int Duration { get; set; }
        [ValidateNever]
        public ICollection<Showtime> Showtimes { get; set; }

    }
}
