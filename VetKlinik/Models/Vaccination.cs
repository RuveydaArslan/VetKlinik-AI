using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models
{
    // Evcil hayvanın aşı kaydı
    public class Vaccination : IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "Evcil Hayvan")]
        public int PetId { get; set; }
        public Pet? Pet { get; set; }

        [Required(ErrorMessage = "Aşı adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Aşı adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Aşı Adı")]
        public string VaccineName { get; set; } = string.Empty; // ör. Kuduz, Karma, Lösemi

        [Required(ErrorMessage = "Yapıldığı tarih zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Yapıldığı Tarih")]
        public DateOnly DateGiven { get; set; }

        // Bir sonraki doz yoksa boş bırakılabilir
        [DataType(DataType.Date)]
        [Display(Name = "Sonraki Doz Tarihi")]
        public DateOnly? NextDoseDate { get; set; }

        [StringLength(300, ErrorMessage = "Not en fazla 300 karakter olabilir.")]
        [Display(Name = "Not")]
        public string? Notes { get; set; }

        // Sunucu tarafı ek kontroller
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateGiven > DateOnly.FromDateTime(DateTime.Today))
            {
                yield return new ValidationResult(
                    "Aşının yapıldığı tarih bugünden sonra olamaz.",
                    new[] { nameof(DateGiven) });
            }

            if (NextDoseDate.HasValue && NextDoseDate.Value <= DateGiven)
            {
                yield return new ValidationResult(
                    "Sonraki doz tarihi, aşının yapıldığı tarihten sonra olmalıdır.",
                    new[] { nameof(NextDoseDate) });
            }
        }
    }
}
