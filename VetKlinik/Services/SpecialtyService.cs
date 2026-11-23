using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Models;

namespace VetKlinik.Services
{
    public class SpecialtyService : ISpecialtyService
    {
        private readonly ApplicationDbContext _context;

        public SpecialtyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<Specialty>> GetAllAsync()
        {
            return _context.Specialties
                .AsNoTracking()
                .Include(s => s.Veterinarians)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public Task<Specialty?> GetByIdAsync(int id)
        {
            return _context.Specialties
                .Include(s => s.Veterinarians)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IslemSonucu> CreateAsync(Specialty uzmanlik)
        {
            if (await _context.Specialties.AnyAsync(s => s.Name == uzmanlik.Name))
                return IslemSonucu.Hata("Bu isimde bir uzmanlık alanı zaten var.");

            _context.Specialties.Add(uzmanlik);
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari($"'{uzmanlik.Name}' uzmanlık alanı eklendi.");
        }

        public async Task<IslemSonucu> UpdateAsync(Specialty uzmanlik)
        {
            var mevcut = await _context.Specialties.FindAsync(uzmanlik.Id);
            if (mevcut == null)
                return IslemSonucu.Hata("Uzmanlık alanı bulunamadı.");

            if (await _context.Specialties.AnyAsync(s => s.Name == uzmanlik.Name && s.Id != uzmanlik.Id))
                return IslemSonucu.Hata("Bu isimde bir uzmanlık alanı zaten var.");

            mevcut.Name = uzmanlik.Name;
            mevcut.Description = uzmanlik.Description;
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari("Uzmanlık alanı güncellendi.");
        }

        public async Task<IslemSonucu> DeleteAsync(int id)
        {
            var uzmanlik = await GetByIdAsync(id);
            if (uzmanlik == null)
                return IslemSonucu.Hata("Uzmanlık alanı bulunamadı.");

            // İş kuralı: hekimlere atanmış uzmanlık silinemez
            if (uzmanlik.Veterinarians.Any())
                return IslemSonucu.Hata("Bu uzmanlık alanına sahip veterinerler olduğu için silinemez.");

            _context.Specialties.Remove(uzmanlik);
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari($"'{uzmanlik.Name}' uzmanlık alanı silindi.");
        }
    }
}
