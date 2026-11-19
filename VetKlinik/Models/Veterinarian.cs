using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models
{
    // Klinikte görev yapan veteriner hekim
    public class Veterinarian
    {
        public int Id { get; set; }

        [Display(Name = "Klinik")]
        public int ClinicId { get; set; }
        public Clinic? Clinic { get; set; }

        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad soyad en fazla 100 karakter olabilir.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Ünvan")]
        public string? Title { get; set; } // ör. "Vet. Hek.", "Doç. Dr."

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [StringLength(20)]
        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [StringLength(1000, ErrorMessage = "Biyografi en fazla 1000 karakter olabilir.")]
        [Display(Name = "Hakkında")]
        public string? Biography { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        // Hekimin sisteme giriş yaptığı kullanıcı hesabı (Veteriner rolünde).
        // Hekim kendi randevularını bu hesap üzerinden görecek.
        [Display(Name = "Kullanıcı Hesabı")]
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // İlişkiler
        public ICollection<Specialty> Specialties { get; set; } = new List<Specialty>();         // çoka-çok
        public ICollection<Service> Services { get; set; } = new List<Service>();                // çoka-çok
        public ICollection<VetAvailability> Availabilities { get; set; } = new List<VetAvailability>(); // bire-çok
    }
}
