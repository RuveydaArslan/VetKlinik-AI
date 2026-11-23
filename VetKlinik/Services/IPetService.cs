using VetKlinik.Models;

namespace VetKlinik.Services
{
    // Evcil hayvan iş kuralları.
    // GÜVENLİK: Üyenin kullandığı tüm metotlar userId alır ve sadece OwnerId == userId olan kayıtlarla çalışır.
    // Böylece bir üye, adres çubuğunda Id değiştirse bile başka üyenin hayvanına ulaşamaz.
    public interface IPetService
    {
        Task<List<Pet>> GetByOwnerAsync(string userId);
        Task<Pet?> GetByIdAsync(int id, string userId);
        Task<Pet?> GetByNameAsync(string ad, string userId);   // MCP: "Pamuk'un aşıları" gibi isimle arama
        Task<bool> IsOwnerAsync(int petId, string userId);     // randevu alırken sahiplik kontrolü

        Task<IslemSonucu> CreateAsync(Pet hayvan, string userId);
        Task<IslemSonucu> UpdateAsync(Pet hayvan, string userId);
        Task<IslemSonucu> DeleteAsync(int id, string userId);

        // Sadece Admin/Veteriner ekranları için: tüm hayvanlar (sahip bilgisiyle)
        Task<List<Pet>> GetAllAsync();
    }
}
