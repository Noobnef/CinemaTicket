using CineTicket.Models.ViewModels;
using CineTicket.Models;
using CineTicket.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CineTicket.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGmailSender _gmailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IGmailSender gmailSender)
        {
            _userManager = userManager;
            _gmailSender = gmailSender;
        }

        [BindProperty]
        public ForgotPasswordViewModel Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email không tồn tại.");
                return Page();
            }

            var otp = new Random().Next(100000, 999999).ToString();
            user.OTP = otp;
            user.OTPGeneratedTime = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await _gmailSender.SendEmailAsync(user.Email, "Mã OTP đặt lại mật khẩu", $"Mã OTP của bạn là: {otp}");

            TempData["Email"] = user.Email;
            return RedirectToPage("ResetPassword");
        }
    }
}
