using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models
{
    // Randevunun yaşam döngüsü:
    // Bekliyor -> (Admin/Veteriner) Onaylandı veya Reddedildi -> Onaylandı ise muayene sonrası Tamamlandı.
    // Hayvan sahibi bekleyen/onaylı randevusunu iptal edebilir -> İptal Edildi.
    public enum AppointmentStatus
    {
        [Display(Name = "Bekliyor")]
        Bekliyor = 0,

        [Display(Name = "Onaylandı")]
        Onaylandi = 1,

        [Display(Name = "Reddedildi")]
        Reddedildi = 2,

        [Display(Name = "Tamamlandı")]
        Tamamlandi = 3,

        [Display(Name = "İptal Edildi")]
        IptalEdildi = 4
    }
}
