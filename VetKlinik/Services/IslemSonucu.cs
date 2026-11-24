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

    // Başarılıysa bir veri de döndüren sürüm (ör. oluşturulan randevu)
    public class IslemSonucu<T> : IslemSonucu
    {
        public T? Veri { get; }

        private IslemSonucu(bool basarili, string mesaj, T? veri) : base(basarili, mesaj)
        {
            Veri = veri;
        }

        public static IslemSonucu<T> Basari(T veri, string mesaj) => new(true, mesaj, veri);
        public new static IslemSonucu<T> Hata(string mesaj) => new(false, mesaj, default);
    }
}
