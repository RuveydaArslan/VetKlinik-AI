using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models
{
    // Veteriner kliniği
    public class Clinic
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Klinik adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Klinik adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Klinik Adı")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adres zorunludur.")]
        [StringLength(250, ErrorMessage = "Adres en fazla 250 karakter olabilir.")]
        [Display(Name = "Adres")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [StringLength(20)]
        [Display(Name = "Telefon")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100)]
        [Display(Name = "E-posta")]
        public string? Email { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        // İlişkiler (bir kliniğin birden çok ...)
        public ICollection<ClinicWorkingHour> WorkingHours { get; set; } = new List<ClinicWorkingHour>();
        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<Veterinarian> Veterinarians { get; set; } = new List<Veterinarian>();
    }
}
