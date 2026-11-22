using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models
{
    // Randevu: hangi hayvan, hangi hekimden, hangi hizmeti, ne zaman alıyor
    public class Appointment
    {
        public int Id { get; set; }

        [Display(Name = "Evcil Hayvan")]
        public int PetId { get; set; }
        public Pet? Pet { get; set; }

        [Display(Name = "Veteriner")]
        public int VeterinarianId { get; set; }
        public Veterinarian? Veterinarian { get; set; }

        [Display(Name = "Hizmet")]
        public int ServiceId { get; set; }
        public Service? Service { get; set; }

        [Display(Name = "Başlangıç")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        // StartTime + hizmet süresi. Çakışma kontrolü [StartTime, EndTime) aralığıyla yapılır.
        [Display(Name = "Bitiş")]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

        // Süre ve ücret randevu anındaki değerlerle KOPYALANIR.
        // Hizmetin fiyatı sonradan değişse bile eski randevunun ücreti değişmez.
        [Display(Name = "Süre (dk)")]
        public int DurationMinutes { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Ücret (₺)")]
        public decimal Price { get; set; }

        // Her randevu "Bekliyor" durumunda oluşur; Admin veya ilgili veteriner onaylar/reddeder
        [Display(Name = "Durum")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Bekliyor;

        [StringLength(500, ErrorMessage = "Not en fazla 500 karakter olabilir.")]
        [Display(Name = "Hayvan Sahibinin Notu")]
        public string? OwnerNote { get; set; }

        [StringLength(300, ErrorMessage = "Red nedeni en fazla 300 karakter olabilir.")]
        [Display(Name = "Red Nedeni")]
        public string? RejectionReason { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Randevu tamamlanınca hekimin girdiği muayene notu (bire-bir)
        public ExaminationNote? ExaminationNote { get; set; }
    }
}
