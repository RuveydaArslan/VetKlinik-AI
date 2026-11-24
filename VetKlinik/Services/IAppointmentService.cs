using VetKlinik.Models;

namespace VetKlinik.Services
{
    // Randevu iş kuralları: uygunluk kontrolü, uygun saatler, oluşturma ve durum değişiklikleri.
    // MVC, REST API ve MCP randevu işlemlerinin TAMAMINI bu servis üzerinden yapar.
    public interface IAppointmentService
    {
        // ---- Kurallar ----
        // Verilen hekim/hizmet/saat için randevu alınabilir mi? Değilse nedenini açıklar.
        Task<IslemSonucu> ValidateAsync(int veterinarianId, int serviceId, DateTime baslangic,
                                        int? petId = null, int? haricRandevuId = null);

        // Seçilen gün ve hizmet için hekimlerin boş saatleri (isteğe bağlı tek hekim)
        Task<List<HekimUygunSaatleri>> GetAvailableSlotsAsync(int serviceId, DateOnly tarih, int? veterinarianId = null);

        // ---- Oluşturma ve durum değişiklikleri ----
        Task<IslemSonucu<Appointment>> CreateAsync(RandevuTalebi talep, string userId);       // Üye: her zaman "Bekliyor"
        Task<IslemSonucu> CancelAsync(int id, string userId);                                  // Üye: kendi randevusunu iptal
        Task<IslemSonucu> ApproveAsync(int id, string userId, bool admin);                     // Admin veya ilgili veteriner
        Task<IslemSonucu> RejectAsync(int id, string? neden, string userId, bool admin);       // Admin veya ilgili veteriner
        Task<IslemSonucu> CompleteAsync(int id, ExaminationNote not, string userId, bool admin); // muayene notuyla tamamla

        // ---- Sorgular ----
        Task<List<Appointment>> GetForOwnerAsync(string userId, bool sadeceYaklasan = false);
        Task<List<Appointment>> GetForVetAsync(int veterinarianId, bool sadeceYaklasan = false);
        Task<List<Appointment>> GetAllAsync(AppointmentStatus? durum = null);
        Task<Appointment?> GetByIdForOwnerAsync(int id, string userId);
        Task<Appointment?> GetByIdForStaffAsync(int id, string userId, bool admin);
    }
}
