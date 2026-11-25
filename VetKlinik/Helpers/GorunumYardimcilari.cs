using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // Haftanın günleri Pazartesi'den başlayarak, Türkçe adlarıyla (açılır liste için)
        public static List<SelectListItem> GunSecenekleri(DayOfWeek? secili = null)
        {
            return HaftaninGunleri()
                .Select(g => new SelectListItem(g.TurkceAd(), ((int)g).ToString(), g == secili))
                .ToList();
        }

        // Pazartesi, Salı, ..., Pazar sırası (DayOfWeek enum'u Pazar'dan başlar)
        public static IEnumerable<DayOfWeek> HaftaninGunleri()
        {
            for (var i = 1; i <= 7; i++)
                yield return (DayOfWeek)(i % 7);
        }

        // Listeleri Pazartesi'den başlayarak sıralamak için anahtar (Pazartesi=0 ... Pazar=6)
        public static int HaftaSirasi(this DayOfWeek gun) => ((int)gun + 6) % 7;
    }
}
