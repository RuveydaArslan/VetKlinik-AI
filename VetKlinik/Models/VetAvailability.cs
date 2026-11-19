using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models
{
    // Hekimin haftalık müsaitlik aralığı.
    // Hayvan sahipleri yalnızca bu aralıklara randevu alabilir.
    public class VetAvailability : IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "Veteriner")]
        public int VeterinarianId { get; set; }
        public Veterinarian? Veterinarian { get; set; }

        [Display(Name = "Gün")]
        public DayOfWeek Day { get; set; }

        [Display(Name = "Başlangıç Saati")]
        [DataType(DataType.Time)]
        public TimeOnly StartTime { get; set; }

        [Display(Name = "Bitiş Saati")]
        [DataType(DataType.Time)]
        public TimeOnly EndTime { get; set; }

        // Sunucu tarafı ek kontrol: bitiş saati başlangıçtan sonra olmalı
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndTime <= StartTime)
            {
                yield return new ValidationResult(
                    "Bitiş saati başlangıç saatinden sonra olmalıdır.",
                    new[] { nameof(EndTime) });
            }
        }
    }
}
