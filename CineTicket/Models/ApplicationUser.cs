using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.ComponentModel.DataAnnotations;

namespace CineTicket.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        public string? EmailConfirmationOTP { get; set; }

        public DateTime? OTPGeneratedTime { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty;

        public string? OTP { get; set; }
    }
}

