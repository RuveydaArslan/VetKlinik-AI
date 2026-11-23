using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Models;

namespace VetKlinik.Services
{
    public class PetService : IPetService
    {
        private readonly ApplicationDbContext _context;

        public PetService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<Pet>> GetByOwnerAsync(string userId)
        {
            return _context.Pets
                .AsNoTracking()
                .Where(p => p.OwnerId == userId)          // <-- sadece kendi hayvanları
                .Include(p => p.Vaccinations)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public Task<Pet?> GetByIdAsync(int id, string userId)
        {
            // Id doğru olsa bile sahibi değilse null döner -> controller 404 (NotFound) verir
            return _context.Pets
                .Include(p => p.Vaccinations)
                .Include(p => p.Appointments).ThenInclude(a => a.Service)
                .Include(p => p.Appointments).ThenInclude(a => a.Veterinarian)
                .Include(p => p.Appointments).ThenInclude(a => a.ExaminationNote)
                .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);
        }

        public Task<Pet?> GetByNameAsync(string ad, string userId)
        {
            // SQL Server varsayılan olarak büyük/küçük harf duyarsız karşılaştırır ("pamuk" = "Pamuk")
            var arananAd = ad.Trim();
            return _context.Pets
                .AsNoTracking()
                .Include(p => p.Vaccinations)
                .FirstOrDefaultAsync(p => p.OwnerId == userId && p.Name == arananAd);
        }

        public Task<bool> IsOwnerAsync(int petId, string userId)
        {
            return _context.Pets.AnyAsync(p => p.Id == petId && p.OwnerId == userId);
        }

        public async Task<IslemSonucu> CreateAsync(Pet hayvan, string userId)
        {
            var kontrol = KontrolEt(hayvan);
            if (!kontrol.Basarili)
                return kontrol;

            // Sahip bilgisi formdan DEĞİL, giriş yapan kullanıcıdan alınır
            hayvan.OwnerId = userId;
            hayvan.CreatedAt = DateTime.Now;

            _context.Pets.Add(hayvan);
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari($"{hayvan.Name} kaydedildi.");
        }

        public async Task<IslemSonucu> UpdateAsync(Pet hayvan, string userId)
        {
            var mevcut = await _context.Pets.FirstOrDefaultAsync(p => p.Id == hayvan.Id && p.OwnerId == userId);
            if (mevcut == null)
                return IslemSonucu.Hata("Evcil hayvan bulunamadı.");

            var kontrol = KontrolEt(hayvan);
            if (!kontrol.Basarili)
                return kontrol;

            mevcut.Name = hayvan.Name;
            mevcut.Species = hayvan.Species;
            mevcut.Breed = hayvan.Breed;
            mevcut.BirthDate = hayvan.BirthDate;
            mevcut.Weight = hayvan.Weight;

            // Yeni fotoğraf yüklendiyse güncelle, yüklenmediyse eskisi kalsın
            if (!string.IsNullOrEmpty(hayvan.PhotoPath))
                mevcut.PhotoPath = hayvan.PhotoPath;

            // OwnerId bilerek güncellenmiyor: hayvan başka bir üyeye aktarılamaz
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari($"{mevcut.Name} güncellendi.");
        }

        public async Task<IslemSonucu> DeleteAsync(int id, string userId)
        {
            var hayvan = await _context.Pets.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);
            if (hayvan == null)
                return IslemSonucu.Hata("Evcil hayvan bulunamadı.");

            // İş kuralı: randevu geçmişi olan hayvan silinmez
            if (await _context.Appointments.AnyAsync(a => a.PetId == id))
                return IslemSonucu.Hata("Bu evcil hayvana ait randevu kayıtları olduğu için silinemez.");

            _context.Pets.Remove(hayvan); // aşı kayıtları cascade ile birlikte silinir
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari($"{hayvan.Name} silindi.");
        }

        public Task<List<Pet>> GetAllAsync()
        {
            return _context.Pets
                .AsNoTracking()
                .Include(p => p.Owner)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        // MVC'deki model doğrulamasından bağımsız olarak, API ve MCP'den gelen veriler de
        // aynı kurallardan geçsin diye kontroller serviste de yapılır
        private static IslemSonucu KontrolEt(Pet hayvan)
        {
            if (string.IsNullOrWhiteSpace(hayvan.Name))
                return IslemSonucu.Hata("Hayvanın adı zorunludur.");

            if (string.IsNullOrWhiteSpace(hayvan.Species))
                return IslemSonucu.Hata("Tür zorunludur.");

            if (hayvan.BirthDate > DateOnly.FromDateTime(DateTime.Today))
                return IslemSonucu.Hata("Doğum tarihi bugünden sonra olamaz.");

            if (hayvan.Weight <= 0 || hayvan.Weight > 200)
                return IslemSonucu.Hata("Kilo 0 ile 200 kg arasında olmalıdır.");

            return IslemSonucu.Basari();
        }
    }
}
