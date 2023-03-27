using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using RestourantFeane.Data;
using RestourantFeane.Models;

namespace RestourantFeane.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _db = db;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Geçerli bir değer giriniz.")]
            [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
            [StringLength(64, MinimumLength = 6, ErrorMessage = "Email Adresi için en az 6 en fazla 64 karakter girebilirsiniz.")]
            [Display(Name = "Email")]
            public string Email { get; set; }
            [Required(ErrorMessage = "Geçerli bir değer giriniz.")]
            [StringLength(16, MinimumLength = 5, ErrorMessage = "Şifre en az 5 en fazla 16 karakter olmalıdır.")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }
            [Required(ErrorMessage = "Geçerli bir değer giriniz.")]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "Paralolar uyuşmuyor.")]
            public string ConfirmPassword { get; set; }
            [Required(ErrorMessage = "Geçerli bir değer giriniz.")]
            [StringLength(30, MinimumLength = 2, ErrorMessage = "Ad için en az 2 en fazla 30 karakter girebilirsiniz.")]
            public string Name { get; set; }
            [Required(ErrorMessage = "Geçerli bir değer giriniz.")]
            [StringLength(30, MinimumLength = 2, ErrorMessage = "Soyad için en az 2 en fazla 30 karakter girebilirsiniz.")]
            public string Surname { get; set; }
            [Required(ErrorMessage = "Geçerli bir değer giriniz.")]
            [StringLength(11, MinimumLength = 11, ErrorMessage = "Telefon numarası 11 karakter olmalıdır.")]
            public string PhoneNumber { get; set; }
            public string Role { get; set; }
            public IEnumerable<SelectListItem> RoleList { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    Name = Input.Name,
                    Surname = Input.Surname,
                    PhoneNumber = Input.PhoneNumber,
                    Role = Input.Role
                };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Yeni bir hesap oluşturmuştur.");
                    if (!await _roleManager.RoleExistsAsync(Other.Role_Admin))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(Other.Role_Admin));
                    }
                    if (!await _roleManager.RoleExistsAsync(Other.Role_User))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(Other.Role_User));
                    }
                    if (!await _roleManager.RoleExistsAsync(Other.Role_Service))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(Other.Role_Service));
                    }

                    if (user.Role == null)
                    {
                        await _userManager.AddToRoleAsync(user, Other.Role_User);
                    }
                    else if (user.Role == Other.Role_User)
                    {
                        await _userManager.AddToRoleAsync(user, Other.Role_User);
                    }
                    else if (user.Role == Other.Role_Service)
                    {
                        await _userManager.AddToRoleAsync(user, Other.Role_Service);
                    }
                    else if (user.Role == Other.Role_Admin)
                    {
                        await _userManager.AddToRoleAsync(user, Other.Role_Admin);
                    }

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Email Doğrulama",
                        $"Hesabınızı doğrulamak için lütfen <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>tıklayın</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        if (user.Role == null)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home", new { Area = "Admin" });
                        }
                    }
                }
                foreach (var error in result.Errors)
                {
                    string errorMessage = error.Description;
                    if (error.Description.EndsWith("is already taken."))
                    {
                        errorMessage = "Zaten böyle bir hesap mevcut";
                    }
                    ModelState.AddModelError("", errorMessage);
                }
            }
            return Page();
        }
    }
}
