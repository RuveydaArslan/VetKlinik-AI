using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models
{
    // Kliniğin haftalık çalışma saatleri (her gün için bir kayıt)
    public class ClinicWorkingHour : IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "Klinik")]
        public int ClinicId { get; set; }
        public Clinic? Clinic { get; set; }

        [Display(Name = "Gün")]
        public DayOfWeek Day { get; set; }

        [Display(Name = "Açılış Saati")]
        [DataType(DataType.Time)]
        public TimeOnly OpenTime { get; set; }

        [Display(Name = "Kapanış Saati")]
        [DataType(DataType.Time)]
        public TimeOnly CloseTime { get; set; }

        // O gün klinik kapalıysa açılış/kapanış saatleri dikkate alınmaz
        [Display(Name = "Kapalı")]
        public bool IsClosed { get; set; }

        // Sunucu tarafı ek kontrol: kapanış saati açılıştan sonra olmalı
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsClosed && CloseTime <= OpenTime)
            {
                yield return new ValidationResult(
                    "Kapanış saati açılış saatinden sonra olmalıdır.",
                    new[] { nameof(CloseTime) });
            }
        }
    }
}
