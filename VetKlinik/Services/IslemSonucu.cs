namespace VetKlinik.Services
{
    // Servis metotlarının "işlem başarılı mı, değilse neden?" bilgisini döndürmek için kullandığı sınıf.
    // Aynı mesaj MVC sayfasında uyarı olarak, API'de 400 cevabında, MCP'de araç cevabında gösterilebilir.
    public class IslemSonucu
    {
        public bool Basarili { get; }
        public string Mesaj { get; }

        protected IslemSonucu(bool basarili, string mesaj)
        {
            Basarili = basarili;
            Mesaj = mesaj;
        }

        public static IslemSonucu Basari(string mesaj = "İşlem başarıyla tamamlandı.") => new(true, mesaj);
        public static IslemSonucu Hata(string mesaj) => new(false, mesaj);
    }
}
