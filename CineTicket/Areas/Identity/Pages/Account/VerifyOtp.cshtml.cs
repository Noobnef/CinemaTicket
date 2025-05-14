using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using CineTicket.Models;

namespace CineTicket.Areas.Identity.Pages.Account
{
    public class VerifyOtpModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public VerifyOtpModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập mã OTP.")]
            public string OTP { get; set; }

            public string UserId { get; set; }
            public string Email { get; set; }
        }

        public void OnGet(string userId, string email, string otp = null)
        {
            Input = new InputModel
            {
                UserId = userId,
                Email = email,
                OTP = otp // Nếu có OTP truyền vào, tự động điền vào input
            };
        }


        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.FindByIdAsync(Input.UserId);

            if (user == null || user.Email != Input.Email)
            {
                ModelState.AddModelError(string.Empty, "Người dùng không hợp lệ.");
                return Page();
            }

            // ✅ Lấy OTP từ database
            if (user.EmailConfirmationOTP != Input.OTP)
            {
                ModelState.AddModelError(string.Empty, "Mã OTP không chính xác.");
                return Page();
            }

            // ✅ Kiểm tra thời gian hết hạn (tùy chọn, ví dụ 10 phút)
            if (user.OTPGeneratedTime.HasValue && user.OTPGeneratedTime.Value.AddMinutes(10) < DateTime.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Mã OTP đã hết hạn. Vui lòng đăng ký lại.");
                return Page();
            }

            user.EmailConfirmed = true;
            user.EmailConfirmationOTP = null;
            user.OTPGeneratedTime = null;

            await _userManager.UpdateAsync(user);

            return RedirectToPage("Login");
        }
    }
}
