using VetKlinik.Models;

namespace VetKlinik.Services
{
    // Veteriner hekim, hekimin uzmanlık/hizmetleri ve müsaitlik saatleri ile ilgili iş kuralları
    public interface IVetService
    {
        Task<List<Veterinarian>> GetAllAsync(bool sadeceAktif = false);
        Task<Veterinarian?> GetByIdAsync(int id);

        // Giriş yapan Veteriner kullanıcısının hekim kaydı
        Task<Veterinarian?> GetByUserIdAsync(string userId);

        // Belirli bir hizmeti verebilen aktif hekimler (randevu alırken kullanılır)
        Task<List<Veterinarian>> GetByServiceAsync(int serviceId);

        Task<IslemSonucu> CreateAsync(Veterinarian hekim, IEnumerable<int> uzmanlikIdleri, IEnumerable<int> hizmetIdleri);
        Task<IslemSonucu> UpdateAsync(Veterinarian hekim, IEnumerable<int> uzmanlikIdleri, IEnumerable<int> hizmetIdleri);
        Task<IslemSonucu> DeleteAsync(int id);

        // Müsaitlik saatleri
        Task<IslemSonucu> AddAvailabilityAsync(VetAvailability musaitlik);
        Task<IslemSonucu> DeleteAvailabilityAsync(int id);

        // Hekime bağlanabilecek (Veteriner rolünde olup başka hekime bağlı olmayan) kullanıcı hesapları
        Task<List<ApplicationUser>> GetAssignableUsersAsync(int? hekimId = null);
    }
}
