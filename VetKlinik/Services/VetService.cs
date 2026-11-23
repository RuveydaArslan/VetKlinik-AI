using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Helpers;
using VetKlinik.Models;

namespace VetKlinik.Services
{
    public class VetService : IVetService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VetService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Task<List<Veterinarian>> GetAllAsync(bool sadeceAktif = false)
        {
            return _context.Veterinarians
                .AsNoTracking()
                .Include(v => v.Clinic)
                .Include(v => v.Specialties)
                .Include(v => v.Services)
                .Where(v => !sadeceAktif || v.IsActive)
                .OrderBy(v => v.FullName)
                .ToListAsync();
        }

        public Task<Veterinarian?> GetByIdAsync(int id)
        {
            return _context.Veterinarians
                .Include(v => v.Clinic)
                .Include(v => v.User)
                .Include(v => v.Specialties)
                .Include(v => v.Services)
                .Include(v => v.Availabilities)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public Task<Veterinarian?> GetByUserIdAsync(string userId)
        {
            return _context.Veterinarians
                .Include(v => v.Clinic)
                .Include(v => v.Specialties)
                .Include(v => v.Services)
                .Include(v => v.Availabilities)
                .FirstOrDefaultAsync(v => v.UserId == userId);
        }

        public Task<List<Veterinarian>> GetByServiceAsync(int serviceId)
        {
            return _context.Veterinarians
                .AsNoTracking()
                .Include(v => v.Availabilities)
                .Include(v => v.Specialties)
                .Where(v => v.IsActive && v.Services.Any(s => s.Id == serviceId))
                .OrderBy(v => v.FullName)
                .ToListAsync();
        }

        public async Task<IslemSonucu> CreateAsync(Veterinarian hekim, IEnumerable<int> uzmanlikIdleri, IEnumerable<int> hizmetIdleri)
        {
            var uIdler = uzmanlikIdleri.ToList();
            var hIdler = hizmetIdleri.ToList();

            var kontrol = await KontrolEtAsync(hekim, hIdler);
            if (!kontrol.Basarili)
                return kontrol;

            // Seçilen Id'lere karşılık gelen uzmanlık ve hizmetleri bağla (çoka-çok)
            hekim.Specialties = await _context.Specialties.Where(s => uIdler.Contains(s.Id)).ToListAsync();
            hekim.Services = await _context.Services.Where(s => hIdler.Contains(s.Id)).ToListAsync();

            _context.Veterinarians.Add(hekim);
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari($"{hekim.FullName} eklendi.");
        }

        public async Task<IslemSonucu> UpdateAsync(Veterinarian hekim, IEnumerable<int> uzmanlikIdleri, IEnumerable<int> hizmetIdleri)
        {
            var mevcut = await _context.Veterinarians
                .Include(v => v.Specialties)
                .Include(v => v.Services)
                .FirstOrDefaultAsync(v => v.Id == hekim.Id);
            if (mevcut == null)
                return IslemSonucu.Hata("Veteriner bulunamadı.");

            var uIdler = uzmanlikIdleri.ToList();
            var hIdler = hizmetIdleri.ToList();

            var kontrol = await KontrolEtAsync(hekim, hIdler);
            if (!kontrol.Basarili)
                return kontrol;

            mevcut.ClinicId = hekim.ClinicId;
            mevcut.FullName = hekim.FullName;
            mevcut.Title = hekim.Title;
            mevcut.Phone = hekim.Phone;
            mevcut.Biography = hekim.Biography;
            mevcut.IsActive = hekim.IsActive;
            mevcut.UserId = hekim.UserId;

            // Çoka-çok ilişkileri yenile: eskileri çıkar, seçilenleri ekle
            mevcut.Specialties.Clear();
            foreach (var u in await _context.Specialties.Where(s => uIdler.Contains(s.Id)).ToListAsync())
                mevcut.Specialties.Add(u);

            mevcut.Services.Clear();
            foreach (var h in await _context.Services.Where(s => hIdler.Contains(s.Id)).ToListAsync())
                mevcut.Services.Add(h);

            await _context.SaveChangesAsync();
            return IslemSonucu.Basari($"{mevcut.FullName} güncellendi.");
        }

        public async Task<IslemSonucu> DeleteAsync(int id)
        {
            var hekim = await _context.Veterinarians.FindAsync(id);
            if (hekim == null)
                return IslemSonucu.Hata("Veteriner bulunamadı.");

            // İş kuralı: randevusu olan hekim silinmez, geçmiş korunur
            if (await _context.Appointments.AnyAsync(a => a.VeterinarianId == id))
                return IslemSonucu.Hata("Bu veterinere ait randevular olduğu için silinemez. Bunun yerine pasif yapabilirsiniz.");

            _context.Veterinarians.Remove(hekim); // müsaitlikler ve ara tablo kayıtları da silinir
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari($"{hekim.FullName} silindi.");
        }

        public async Task<IslemSonucu> AddAvailabilityAsync(VetAvailability musaitlik)
        {
            var hekim = await _context.Veterinarians
                .Include(v => v.Clinic!)
                    .ThenInclude(c => c.WorkingHours)
                .FirstOrDefaultAsync(v => v.Id == musaitlik.VeterinarianId);
            if (hekim == null)
                return IslemSonucu.Hata("Veteriner bulunamadı.");

            if (musaitlik.EndTime <= musaitlik.StartTime)
                return IslemSonucu.Hata("Bitiş saati başlangıç saatinden sonra olmalıdır.");

            // Kural 1: Müsaitlik, kliniğin o günkü çalışma saatleri içinde olmalı
            var gunAdi = musaitlik.Day.TurkceAd();
            var klinikSaati = hekim.Clinic!.WorkingHours.FirstOrDefault(w => w.Day == musaitlik.Day);
            if (klinikSaati == null || klinikSaati.IsClosed)
                return IslemSonucu.Hata($"Klinik {gunAdi} günü kapalı.");

            if (musaitlik.StartTime < klinikSaati.OpenTime || musaitlik.EndTime > klinikSaati.CloseTime)
                return IslemSonucu.Hata(
                    $"Müsaitlik, kliniğin {gunAdi} çalışma saatleri ({klinikSaati.OpenTime:HH:mm}–{klinikSaati.CloseTime:HH:mm}) dışında olamaz.");

            // Kural 2: Aynı gün içindeki başka bir müsaitlik aralığıyla çakışmamalı
            // (iki aralık çakışır <=> yeniBaşlangıç < eskiBitiş VE eskiBaşlangıç < yeniBitiş)
            var cakisma = await _context.VetAvailabilities.AnyAsync(a =>
                a.VeterinarianId == musaitlik.VeterinarianId &&
                a.Day == musaitlik.Day &&
                musaitlik.StartTime < a.EndTime &&
                a.StartTime < musaitlik.EndTime);
            if (cakisma)
                return IslemSonucu.Hata($"{gunAdi} günü için bu saatlerle çakışan bir müsaitlik zaten var.");

            _context.VetAvailabilities.Add(musaitlik);
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari($"{gunAdi} {musaitlik.StartTime:HH:mm}–{musaitlik.EndTime:HH:mm} müsaitliği eklendi.");
        }

        public async Task<IslemSonucu> DeleteAvailabilityAsync(int id)
        {
            var musaitlik = await _context.VetAvailabilities.FindAsync(id);
            if (musaitlik == null)
                return IslemSonucu.Hata("Müsaitlik kaydı bulunamadı.");

            _context.VetAvailabilities.Remove(musaitlik);
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari("Müsaitlik kaydı silindi.");
        }

        public async Task<List<ApplicationUser>> GetAssignableUsersAsync(int? hekimId = null)
        {
            var veterinerKullanicilari = await _userManager.GetUsersInRoleAsync(Roles.Veteriner);

            // Başka bir hekime zaten bağlı olan hesaplar listeden çıkarılır
            var bagliHesaplar = await _context.Veterinarians
                .Where(v => v.UserId != null && v.Id != hekimId)
                .Select(v => v.UserId!)
                .ToListAsync();

            return veterinerKullanicilari
                .Where(u => !bagliHesaplar.Contains(u.Id))
                .OrderBy(u => u.FullName)
                .ToList();
        }

        // Ekleme ve güncellemede ortak kontroller
        private async Task<IslemSonucu> KontrolEtAsync(Veterinarian hekim, IEnumerable<int> hizmetIdleri)
        {
            if (!await _context.Clinics.AnyAsync(c => c.Id == hekim.ClinicId))
                return IslemSonucu.Hata("Seçilen klinik bulunamadı.");

            // Hekim yalnızca kendi kliniğinin hizmetlerini verebilir
            var idler = hizmetIdleri.Distinct().ToList();
            var gecerliHizmetSayisi = await _context.Services
                .CountAsync(s => idler.Contains(s.Id) && s.ClinicId == hekim.ClinicId);
            if (gecerliHizmetSayisi != idler.Count)
                return IslemSonucu.Hata("Seçilen hizmetler hekimin çalıştığı kliniğe ait olmalıdır.");

            // Bağlanacak kullanıcı hesabı kontrolleri
            if (!string.IsNullOrEmpty(hekim.UserId))
            {
                var kullanici = await _userManager.FindByIdAsync(hekim.UserId);
                if (kullanici == null)
                    return IslemSonucu.Hata("Seçilen kullanıcı hesabı bulunamadı.");

                if (!await _userManager.IsInRoleAsync(kullanici, Roles.Veteriner))
                    return IslemSonucu.Hata("Hekime yalnızca Veteriner rolündeki bir hesap bağlanabilir.");

                var baskaHekimde = await _context.Veterinarians
                    .AnyAsync(v => v.UserId == hekim.UserId && v.Id != hekim.Id);
                if (baskaHekimde)
                    return IslemSonucu.Hata("Bu kullanıcı hesabı başka bir hekime bağlı.");
            }

            return IslemSonucu.Basari();
        }
    }
}
