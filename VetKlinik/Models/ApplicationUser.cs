using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VetKlinik.Models
{
    // Identity'nin hazır kullanıcı sınıfını (IdentityUser) genişletiyoruz.
    // IdentityUser zaten Id, UserName, Email, PasswordHash gibi alanları içerir;
    // biz sadece projeye özel alanları ekliyoruz.
    public class ApplicationUser : IdentityUser
    {
        // Kullanıcının adı soyadı (kayıt formuna Gün 5'te eklenecek)
        [StringLength(100)]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;
    }
}
