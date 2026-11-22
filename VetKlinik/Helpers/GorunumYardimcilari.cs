using System.ComponentModel.DataAnnotations;
using System.Reflection;
using VetKlinik.Models;

namespace VetKlinik.Helpers
{
    // View'larda tekrar tekrar kullanılan küçük yardımcı metotlar
    public static class GorunumYardimcilari
    {
        // Enum değerinin [Display(Name = "...")] ile verilen Türkçe adını döndürür
        // ör. AppointmentStatus.Onaylandi -> "Onaylandı"
        public static string GorunenAd(this Enum deger)
        {
            var alan = deger.GetType().GetField(deger.ToString());
            return alan?.GetCustomAttribute<DisplayAttribute>()?.Name ?? deger.ToString();
        }

        // Randevu durumuna göre Bootstrap rozet (badge) rengi
        public static string RozetSinifi(this AppointmentStatus durum) => durum switch
        {
            AppointmentStatus.Bekliyor    => "bg-warning text-dark",
            AppointmentStatus.Onaylandi   => "bg-primary",
            AppointmentStatus.Tamamlandi  => "bg-success",
            AppointmentStatus.Reddedildi  => "bg-danger",
            AppointmentStatus.IptalEdildi => "bg-secondary",
            _ => "bg-light text-dark"
        };

        // Haftanın gününün Türkçe adı
        public static string TurkceAd(this DayOfWeek gun) => gun switch
        {
            DayOfWeek.Monday    => "Pazartesi",
            DayOfWeek.Tuesday   => "Salı",
            DayOfWeek.Wednesday => "Çarşamba",
            DayOfWeek.Thursday  => "Perşembe",
            DayOfWeek.Friday    => "Cuma",
            DayOfWeek.Saturday  => "Cumartesi",
            DayOfWeek.Sunday    => "Pazar",
            _ => gun.ToString()
        };
    }
}
