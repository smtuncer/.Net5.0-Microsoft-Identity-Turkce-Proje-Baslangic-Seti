using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using RestourantFeane.Data;
using RestourantFeane.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RestourantFeane.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Telefon Numarası")]
            [Required(ErrorMessage = "Geçerli bir değer giriniz.")]
            [StringLength(11, MinimumLength = 11, ErrorMessage = "Telefon numarası 11 karakter olmalıdır.")]
            public string PhoneNumber { get; set; }
            [Required(ErrorMessage = "Geçerli bir değer giriniz.")]
            [StringLength(30, MinimumLength = 2, ErrorMessage = "Ad için en az 2 en fazla 30 karakter girebilirsiniz.")]
            public string Name { get; set; }
            [Required(ErrorMessage = "Geçerli bir değer giriniz.")]
            [StringLength(30, MinimumLength = 2, ErrorMessage = "Soyad için en az 2 en fazla 30 karakter girebilirsiniz.")]
            public string Surname { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var name = user.Name;
            var Surname = user.Surname;
            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                Name = name,
                Surname = Surname,
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var data = _context.ApplicationUsers.FirstOrDefault(x => x.Id == user.Id);
            await LoadAsync(data);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var data = _context.ApplicationUsers.FirstOrDefault(x => x.Id == user.Id);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(data);
                return Page();
            }

            if (Input.Name != data.Name)
            {
                data.Name = Input.Name;
                await _userManager.UpdateAsync(data);
            }
            if (Input.Surname != data.Surname)
            {
                data.Surname = Input.Surname;
                await _userManager.UpdateAsync(data);
            }
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Telefon numarasını ayarlamaya çalışırken beklenmeyen bir hata oluştu.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Profiliniz Güncellendi";
            return RedirectToPage();
        }
    }
}
