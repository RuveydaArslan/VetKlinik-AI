using VetKlinik.Models;

namespace VetKlinik.Services
{
    // Klinik ve çalışma saatleri ile ilgili iş kuralları
    public interface IClinicService
    {
        Task<List<Clinic>> GetAllAsync();
        Task<Clinic?> GetByIdAsync(int id);
        Task<IslemSonucu> CreateAsync(Clinic klinik);
        Task<IslemSonucu> UpdateAsync(Clinic klinik);
        Task<IslemSonucu> DeleteAsync(int id);

        // Çalışma saatleri (her gün için tek kayıt: varsa günceller, yoksa ekler)
        Task<IslemSonucu> SaveWorkingHourAsync(ClinicWorkingHour saat);
        Task<IslemSonucu> DeleteWorkingHourAsync(int id);

        // Klinik verilen zaman aralığında açık mı? (randevu kontrolünde kullanılır)
        Task<bool> IsOpenAsync(int clinicId, DateTime baslangic, DateTime bitis);
    }
}
