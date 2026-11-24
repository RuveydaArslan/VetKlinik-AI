namespace VetKlinik.Services
{
    // GetAvailableSlotsAsync sonucu: bir hekim ve o gün boş olan başlangıç saatleri
    public class HekimUygunSaatleri
    {
        public int VeterinarianId { get; set; }
        public string HekimAdi { get; set; } = string.Empty;
        public List<DateTime> BosSaatler { get; set; } = new();
    }
}
