using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Helpers;
using VetKlinik.Models;

namespace VetKlinik.Services
{
    public class ClinicService : IClinicService
    {
        private readonly ApplicationDbContext _context;

        public ClinicService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<Clinic>> GetAllAsync()
        {
            return _context.Clinics
                .AsNoTracking()                 // sadece okuma: değişiklik takibine gerek yok (daha hızlı)
                .Include(c => c.WorkingHours)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public Task<Clinic?> GetByIdAsync(int id)
        {
            return _context.Clinics
                .Include(c => c.WorkingHours)
                .Include(c => c.Services)
                .Include(c => c.Veterinarians)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IslemSonucu> CreateAsync(Clinic klinik)
        {
            _context.Clinics.Add(klinik);
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari($"'{klinik.Name}' kliniği eklendi.");
        }

        public async Task<IslemSonucu> UpdateAsync(Clinic klinik)
        {
            var mevcut = await _context.Clinics.FindAsync(klinik.Id);
            if (mevcut == null)
                return IslemSonucu.Hata("Klinik bulunamadı.");

            // Sadece formdan değişmesine izin verilen alanları güncelle
            mevcut.Name = klinik.Name;
            mevcut.Address = klinik.Address;
            mevcut.Phone = klinik.Phone;
            mevcut.Email = klinik.Email;
            mevcut.Description = klinik.Description;

            await _context.SaveChangesAsync();
            return IslemSonucu.Basari("Klinik bilgileri güncellendi.");
        }

        public async Task<IslemSonucu> DeleteAsync(int id)
        {
            var klinik = await GetByIdAsync(id);
            if (klinik == null)
                return IslemSonucu.Hata("Klinik bulunamadı.");

            // İş kuralı: hizmeti veya hekimi olan klinik silinemez
            if (klinik.Services.Any() || klinik.Veterinarians.Any())
                return IslemSonucu.Hata("Bu kliniğe bağlı hizmet veya veteriner olduğu için silinemez. Önce onları kaldırın.");

            _context.Clinics.Remove(klinik); // çalışma saatleri cascade ile birlikte silinir
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari($"'{klinik.Name}' kliniği silindi.");
        }

        public async Task<IslemSonucu> SaveWorkingHourAsync(ClinicWorkingHour saat)
        {
            if (!await _context.Clinics.AnyAsync(c => c.Id == saat.ClinicId))
                return IslemSonucu.Hata("Klinik bulunamadı.");

            if (!saat.IsClosed && saat.CloseTime <= saat.OpenTime)
                return IslemSonucu.Hata("Kapanış saati açılış saatinden sonra olmalıdır.");

            // Aynı gün için kayıt varsa güncelle, yoksa yeni ekle
            var mevcut = await _context.ClinicWorkingHours
                .FirstOrDefaultAsync(w => w.ClinicId == saat.ClinicId && w.Day == saat.Day);

            if (mevcut == null)
            {
                _context.ClinicWorkingHours.Add(saat);
            }
            else
            {
                mevcut.OpenTime = saat.OpenTime;
                mevcut.CloseTime = saat.CloseTime;
                mevcut.IsClosed = saat.IsClosed;
            }

            await _context.SaveChangesAsync();
            return IslemSonucu.Basari($"{saat.Day.TurkceAd()} günü çalışma saati kaydedildi.");
        }

        public async Task<IslemSonucu> DeleteWorkingHourAsync(int id)
        {
            var saat = await _context.ClinicWorkingHours.FindAsync(id);
            if (saat == null)
                return IslemSonucu.Hata("Çalışma saati bulunamadı.");

            _context.ClinicWorkingHours.Remove(saat);
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari("Çalışma saati silindi.");
        }

        public async Task<bool> IsOpenAsync(int clinicId, DateTime baslangic, DateTime bitis)
        {
            // Randevu tek bir gün içinde olmalı
            if (baslangic.Date != bitis.Date)
                return false;

            var gun = baslangic.DayOfWeek;
            var saat = await _context.ClinicWorkingHours
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.ClinicId == clinicId && w.Day == gun);

            // O gün için kayıt yoksa veya klinik kapalıysa
            if (saat == null || saat.IsClosed)
                return false;

            var bas = TimeOnly.FromDateTime(baslangic);
            var bit = TimeOnly.FromDateTime(bitis);
            return bas >= saat.OpenTime && bit <= saat.CloseTime;
        }
    }
}
