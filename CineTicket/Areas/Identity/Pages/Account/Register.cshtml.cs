using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using CineTicket.Models;
using CineTicket.Areas.Admin.Models;
using Hangfire;
using CineTicket.Repositories;
using System.ComponentModel.DataAnnotations;

namespace CineTicket.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IGmailSender _gmailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IGmailSender gmailSender)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _gmailSender = gmailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            public string FullName { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; }

            public string? Role { get; set; }

            [ValidateNever]
            public IEnumerable<SelectListItem> RoleList { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).Result)
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Company));
            }

            Input = new InputModel
            {
                RoleList = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                })
            };

            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();
                user.FullName = Input.FullName;
                user.Role = Input.Role ?? SD.Role_Customer;
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(Input.Role))
                        await _userManager.AddToRoleAsync(user, Input.Role);
                    else
                        await _userManager.AddToRoleAsync(user, SD.Role_Customer);

                    string otp = new Random().Next(100000, 999999).ToString();
                    user.EmailConfirmationOTP = otp;
                    user.OTPGeneratedTime = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    var verifyUrl = Url.Page(
                        "/Account/VerifyOtp",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, email = user.Email },
                        protocol: Request.Scheme
                    );

                    var emailContent = $@"
<div style='max-width: 600px; margin: auto; font-family: Segoe UI, sans-serif; background: #f7f7f7; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
    <div style='background: #dc3545; padding: 24px; text-align: center; color: white;'>
        <h1 style='margin: 0; font-size: 28px;'>🎬 CineTicket</h1>
        <p style='margin: 0; font-size: 16px;'>Xác nhận tài khoản của bạn</p>
    </div>
    <div style='padding: 30px; background-color: #ffffff; text-align: center;'>
        <p style='font-size: 16px;'>Chào <b style='color: #007bff;'>{user.Email}</b>,</p>
        <p style='margin-top: 8px;'>Cảm ơn bạn đã đăng ký. Vui lòng sử dụng mã OTP bên dưới để xác nhận tài khoản:</p>

        <div style='margin: 20px auto; font-size: 32px; font-weight: bold; color: #dc3545;'>{otp}</div>

        <a href='{verifyUrl}' style='
            display: inline-block;
            margin-top: 20px;
            padding: 14px 28px;
            background-color: #dc3545;
            color: white;
            font-size: 16px;
            font-weight: 600;
            text-decoration: none;
            border-radius: 8px;
            transition: background-color 0.3s ease;
        '>Xác nhận tài khoản</a>

        <p style='margin-top: 30px; font-size: 13px; color: #6c757d;'>Mã OTP sẽ hết hạn sau 10 phút. Nếu bạn không yêu cầu tạo tài khoản, vui lòng bỏ qua email này.</p>
    </div>
    <div style='background-color: #f1f1f1; text-align: center; padding: 12px; font-size: 12px; color: #888;'>
        © 2025 CineTicket. All rights reserved.
    </div>
</div>";

                    BackgroundJob.Enqueue(() =>
                        _gmailSender.SendEmail(user.Email, "Mã OTP xác nhận tài khoản", emailContent));

                    return RedirectToPage("VerifyOtp", new { area = "Identity", userId = user.Id, email = user.Email });
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Không thể tạo instance của '{nameof(ApplicationUser)}'.");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException("UserStore không hỗ trợ email.");
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
