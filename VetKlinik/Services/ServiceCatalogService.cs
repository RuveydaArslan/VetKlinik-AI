using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Models;

namespace VetKlinik.Services
{
    public class ServiceCatalogService : IServiceCatalogService
    {
        private readonly ApplicationDbContext _context;

        public ServiceCatalogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<Service>> GetAllAsync(bool sadeceAktif = false)
        {
            var sorgu = _context.Services.AsNoTracking().Include(s => s.Clinic).AsQueryable();

            if (sadeceAktif)
                sorgu = sorgu.Where(s => s.IsActive);

            return sorgu.OrderBy(s => s.Name).ToListAsync();
        }

        public Task<List<Service>> GetByClinicAsync(int clinicId, bool sadeceAktif = true)
        {
            return _context.Services
                .AsNoTracking()
                .Where(s => s.ClinicId == clinicId && (!sadeceAktif || s.IsActive))
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public Task<Service?> GetByIdAsync(int id)
        {
            return _context.Services
                .Include(s => s.Clinic)
                .Include(s => s.Veterinarians)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IslemSonucu> CreateAsync(Service hizmet)
        {
            var kontrol = await KontrolEtAsync(hizmet);
            if (!kontrol.Basarili)
                return kontrol;

            _context.Services.Add(hizmet);
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari($"'{hizmet.Name}' hizmeti eklendi.");
        }

        public async Task<IslemSonucu> UpdateAsync(Service hizmet)
        {
            var mevcut = await _context.Services
                .Include(s => s.Veterinarians)
                .FirstOrDefaultAsync(s => s.Id == hizmet.Id);
            if (mevcut == null)
                return IslemSonucu.Hata("Hizmet bulunamadı.");

            // İş kuralı: hekimlere atanmış bir hizmet başka bir kliniğe taşınamaz
            if (mevcut.ClinicId != hizmet.ClinicId && mevcut.Veterinarians.Any())
                return IslemSonucu.Hata("Bu hizmet hekimlere atanmış olduğu için kliniği değiştirilemez.");

            var kontrol = await KontrolEtAsync(hizmet);
            if (!kontrol.Basarili)
                return kontrol;

            mevcut.ClinicId = hizmet.ClinicId;
            mevcut.Name = hizmet.Name;
            mevcut.Description = hizmet.Description;
            mevcut.DurationMinutes = hizmet.DurationMinutes;
            mevcut.Price = hizmet.Price;
            mevcut.IsActive = hizmet.IsActive;

            // Not: Ücret/süre değişikliği eski randevuları etkilemez (randevuda kopyası tutuluyor)
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari("Hizmet güncellendi.");
        }

        public async Task<IslemSonucu> DeleteAsync(int id)
        {
            var hizmet = await _context.Services.FindAsync(id);
            if (hizmet == null)
                return IslemSonucu.Hata("Hizmet bulunamadı.");

            // İş kuralı: randevusu olan hizmet silinmez, geçmiş korunur
            if (await _context.Appointments.AnyAsync(a => a.ServiceId == id))
                return IslemSonucu.Hata("Bu hizmete ait randevular olduğu için silinemez. Bunun yerine hizmeti pasif yapabilirsiniz.");

            _context.Services.Remove(hizmet); // hekim-hizmet ara tablo kayıtları otomatik silinir
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari($"'{hizmet.Name}' hizmeti silindi.");
        }

        // Ekleme ve güncellemede ortak kontroller
        private async Task<IslemSonucu> KontrolEtAsync(Service hizmet)
        {
            if (!await _context.Clinics.AnyAsync(c => c.Id == hizmet.ClinicId))
                return IslemSonucu.Hata("Seçilen klinik bulunamadı.");

            if (hizmet.DurationMinutes < 5 || hizmet.DurationMinutes > 480)
                return IslemSonucu.Hata("Süre 5 ile 480 dakika arasında olmalıdır.");

            if (hizmet.Price < 0)
                return IslemSonucu.Hata("Ücret negatif olamaz.");

            // Aynı klinikte aynı isimde iki hizmet olmasın
            var ayniIsim = await _context.Services.AnyAsync(s =>
                s.ClinicId == hizmet.ClinicId && s.Name == hizmet.Name && s.Id != hizmet.Id);
            if (ayniIsim)
                return IslemSonucu.Hata("Bu klinikte aynı isimde bir hizmet zaten var.");

            return IslemSonucu.Basari();
        }
    }
}
