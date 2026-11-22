using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models
{
    // Tamamlanan bir randevu için hekimin girdiği muayene notu.
    // Her randevunun en fazla bir muayene notu olur (bire-bir ilişki).
    public class ExaminationNote
    {
        public int Id { get; set; }

        [Display(Name = "Randevu")]
        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        [StringLength(500, ErrorMessage = "Şikâyet en fazla 500 karakter olabilir.")]
        [Display(Name = "Şikâyet")]
        public string? Complaint { get; set; }

        [Required(ErrorMessage = "Teşhis zorunludur.")]
        [StringLength(500, ErrorMessage = "Teşhis en fazla 500 karakter olabilir.")]
        [Display(Name = "Teşhis")]
        public string Diagnosis { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Tedavi en fazla 1000 karakter olabilir.")]
        [Display(Name = "Uygulanan Tedavi")]
        public string? Treatment { get; set; }

        [StringLength(2000, ErrorMessage = "Not en fazla 2000 karakter olabilir.")]
        [Display(Name = "Hekim Notu")]
        public string? Notes { get; set; }

        [Display(Name = "Muayene Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
