using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestourantFeane.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Geçerli bir değer giriniz.")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Ad için en az 2 en fazla 30 karakter girebilirsiniz.")]
        public string Name{ get; set; }
        [Required(ErrorMessage = "Geçerli bir değer giriniz.")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Soyad için en az 2 en fazla 30 karakter girebilirsiniz.")]
        public string Surname{ get; set; }
        [NotMapped]
        public string Role { get; set; }
    }
}
