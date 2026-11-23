using VetKlinik.Models;

namespace VetKlinik.Services
{
    // Hekim uzmanlık alanları ile ilgili iş kuralları
    public interface ISpecialtyService
    {
        Task<List<Specialty>> GetAllAsync();
        Task<Specialty?> GetByIdAsync(int id);
        Task<IslemSonucu> CreateAsync(Specialty uzmanlik);
        Task<IslemSonucu> UpdateAsync(Specialty uzmanlik);
        Task<IslemSonucu> DeleteAsync(int id);
    }
}
