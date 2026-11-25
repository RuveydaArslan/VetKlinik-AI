using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetKlinik.Models;

namespace VetKlinik.ViewModels
{
    // Admin > Veteriner ekleme/düzenleme formu.
    // Entity (Veterinarian) yerine ViewModel kullanıyoruz çünkü formda entity'de olmayan alanlar var:
    // seçilen uzmanlık/hizmet Id listeleri, yeni hesap bilgileri ve açılır listelerin verileri.
    public class VeterinerFormViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Lütfen bir klinik seçin.")]
        [Display(Name = "Klinik")]
        public int ClinicId { get; set; }

        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad soyad en fazla 100 karakter olabilir.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Ünvan en fazla 50 karakter olabilir.")]
        [Display(Name = "Ünvan")]
        public string? Title { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [StringLength(20)]
        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [StringLength(1000, ErrorMessage = "En fazla 1000 karakter olabilir.")]
        [Display(Name = "Hakkında")]
        public string? Biography { get; set; }

        [Display(Name = "Aktif (randevu kabul ediyor)")]
        public bool IsActive { get; set; } = true;

        // ---- Giriş hesabı: mevcut bir Veteriner hesabını seç VEYA yeni hesap oluştur ----
        [Display(Name = "Mevcut Hesap")]
        public string? UserId { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "Yeni Hesap E-postası")]
        public string? YeniHesapEmail { get; set; }

        [StringLength(100, MinimumLength = 3, ErrorMessage = "Şifre en az 3 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Hesap Şifresi")]
        public string? YeniHesapSifre { get; set; }

        // ---- Checkbox listelerinden gelen seçimler ----
        public List<int> SecilenUzmanlikIdleri { get; set; } = new();
        public List<int> SecilenHizmetIdleri { get; set; } = new();

        // ---- Sadece ekranı çizmek için (formdan geri gelmez, doğrulanmaz) ----
        [ValidateNever] public List<Specialty> TumUzmanliklar { get; set; } = new();
        [ValidateNever] public List<Service> TumHizmetler { get; set; } = new();
        [ValidateNever] public SelectList? Klinikler { get; set; }
        [ValidateNever] public SelectList? Hesaplar { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var emailVar = !string.IsNullOrWhiteSpace(YeniHesapEmail);
            var sifreVar = !string.IsNullOrWhiteSpace(YeniHesapSifre);

            if (emailVar && !sifreVar)
                yield return new ValidationResult("Yeni hesap için şifre de girilmelidir.", new[] { nameof(YeniHesapSifre) });

            if (sifreVar && !emailVar)
                yield return new ValidationResult("Yeni hesap için e-posta da girilmelidir.", new[] { nameof(YeniHesapEmail) });

            if (emailVar && !string.IsNullOrEmpty(UserId))
                yield return new ValidationResult("Hem mevcut hesap seçip hem yeni hesap oluşturamazsınız.", new[] { nameof(UserId) });
        }
    }
}
