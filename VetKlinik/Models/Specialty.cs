using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models
{
    // Hekim uzmanlık alanı (küçük hayvan, egzotik hayvan, cerrahi, dermatoloji ...)
    public class Specialty
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Uzmanlık adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Uzmanlık adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Uzmanlık Alanı")]
        public string Name { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Açıklama en fazla 300 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        // Bu uzmanlığa sahip hekimler (çoka-çok)
        public ICollection<Veterinarian> Veterinarians { get; set; } = new List<Veterinarian>();
    }
}
