using System.ComponentModel.DataAnnotations;

namespace CineTicket.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

}
