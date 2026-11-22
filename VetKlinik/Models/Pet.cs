using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VetKlinik.Models
{
    // Hayvan sahibine (Üye) ait evcil hayvan
    public class Pet : IValidatableObject
    {
        public int Id { get; set; }

        // Hayvanın sahibi. Formdan GELMEZ; controller'da giriş yapan kullanıcının Id'si atanır.
        // Bir üyenin başka üyenin hayvanını görememesi bu alana göre filtrelenerek sağlanır.
        [ValidateNever]
        public string OwnerId { get; set; } = string.Empty;
        public ApplicationUser? Owner { get; set; }

        [Required(ErrorMessage = "Hayvanın adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        [Display(Name = "Adı")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tür zorunludur.")]
        [StringLength(50, ErrorMessage = "Tür en fazla 50 karakter olabilir.")]
        [Display(Name = "Türü")]
        public string Species { get; set; } = string.Empty; // ör. Kedi, Köpek, Kuş, Tavşan

        [StringLength(50, ErrorMessage = "Irk en fazla 50 karakter olabilir.")]
        [Display(Name = "Irkı")]
        public string? Breed { get; set; }

        [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateOnly BirthDate { get; set; }

        [Range(0.01, 200, ErrorMessage = "Kilo 0,01 ile 200 kg arasında olmalıdır.")]
        [Display(Name = "Kilo (kg)")]
        public decimal Weight { get; set; }

        // wwwroot altına kaydedilen fotoğrafın yolu (ör. /uploads/pets/abc.jpg) - isteğe bağlı
        [StringLength(260)]
        [Display(Name = "Fotoğraf")]
        public string? PhotoPath { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // İlişkiler
        public ICollection<Vaccination> Vaccinations { get; set; } = new List<Vaccination>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Sunucu tarafı ek kontrol: doğum tarihi gelecekte olamaz
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (BirthDate > DateOnly.FromDateTime(DateTime.Today))
            {
                yield return new ValidationResult(
                    "Doğum tarihi bugünden sonra olamaz.",
                    new[] { nameof(BirthDate) });
            }
        }
    }
}
