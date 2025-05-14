using CineTicket.Models.ViewModels;
using CineTicket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CineTicket.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public ResetPasswordViewModel Input { get; set; } = new();

        public void OnGet()
        {
            if (TempData["Email"] is string email)
                Input.Email = email;
        }

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

            if (user.OTP != Input.OTP || (DateTime.UtcNow - user.OTPGeneratedTime)?.Minutes > 5)
            {
                ModelState.AddModelError(string.Empty, "Mã OTP không đúng hoặc đã hết hạn.");
                return Page();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, Input.NewPassword);

            if (result.Succeeded)
            {
                user.OTP = null;
                user.OTPGeneratedTime = null;
                await _userManager.UpdateAsync(user);
                return RedirectToPage("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }
    }
}
