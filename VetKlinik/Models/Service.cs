using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models
{
    // Klinikte sunulan hizmet (muayene, aşı, kısırlaştırma, diş temizliği, tıraş ve bakım ...)
    public class Service
    {
        public int Id { get; set; }

        [Display(Name = "Klinik")]
        public int ClinicId { get; set; }
        public Clinic? Clinic { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Hizmet adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Hizmet Adı")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        // Randevu çakışma hesabında kullanılacak süre
        [Range(5, 480, ErrorMessage = "Süre 5 ile 480 dakika arasında olmalıdır.")]
        [Display(Name = "Süre (dk)")]
        public int DurationMinutes { get; set; }

        [Range(0, 100000, ErrorMessage = "Ücret 0 ile 100.000 ₺ arasında olmalıdır.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Ücret (₺)")]
        public decimal Price { get; set; }

        // Pasif hizmetler için yeni randevu alınamaz
        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        // Bu hizmeti verebilen hekimler (çoka-çok)
        public ICollection<Veterinarian> Veterinarians { get; set; } = new List<Veterinarian>();
    }
}
