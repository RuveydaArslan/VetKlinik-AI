using VetKlinik.Models;

namespace VetKlinik.Services
{
    // Klinik hizmetleri (muayene, aşı, kısırlaştırma ...) ile ilgili iş kuralları
    public interface IServiceCatalogService
    {
        Task<List<Service>> GetAllAsync(bool sadeceAktif = false);
        Task<List<Service>> GetByClinicAsync(int clinicId, bool sadeceAktif = true);
        Task<Service?> GetByIdAsync(int id);
        Task<IslemSonucu> CreateAsync(Service hizmet);
        Task<IslemSonucu> UpdateAsync(Service hizmet);
        Task<IslemSonucu> DeleteAsync(int id);
    }
}
