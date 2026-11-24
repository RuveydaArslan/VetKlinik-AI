using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Helpers;
using VetKlinik.Models;

namespace VetKlinik.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IVetService _vetService;
        private readonly IPetService _petService;

        public AppointmentService(ApplicationDbContext context, IVetService vetService, IPetService petService)
        {
            _context = context;
            _vetService = vetService;
            _petService = petService;
        }

        // =====================================================================
        // 1) UYGUNLUK KONTROLÜ
        // =====================================================================
        public async Task<IslemSonucu> ValidateAsync(int veterinarianId, int serviceId, DateTime baslangic,
                                                     int? petId = null, int? haricRandevuId = null)
        {
            // --- Adım 0: Hizmet ve hekim var mı, aktif mi? ---
            var hizmet = await _context.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == serviceId);
            if (hizmet == null || !hizmet.IsActive)
                return IslemSonucu.Hata("Seçilen hizmet bulunamadı veya şu an verilmiyor.");

            var hekim = await _context.Veterinarians
                .AsNoTracking()
                .Include(v => v.Services)
                .Include(v => v.Availabilities)
                .FirstOrDefaultAsync(v => v.Id == veterinarianId);
            if (hekim == null || !hekim.IsActive)
                return IslemSonucu.Hata("Seçilen veteriner bulunamadı veya şu an randevu kabul etmiyor.");

            var hekimAdi = HekimAdi(hekim);

            // --- Adım 1: Hekim bu hizmeti veriyor mu? ---
            if (!hekim.Services.Any(s => s.Id == serviceId))
                return IslemSonucu.Hata($"{hekimAdi} '{hizmet.Name}' hizmetini vermiyor.");

            // Bitiş saati = başlangıç + hizmet süresi (çakışma hesabına süre böyle dahil edilir)
            var bitis = baslangic.AddMinutes(hizmet.DurationMinutes);
            var gun = baslangic.DayOfWeek;
            var bas = TimeOnly.FromDateTime(baslangic);
            var bit = TimeOnly.FromDateTime(bitis);

            // --- Adım 2: Tarih geçmişte mi? ---
            if (baslangic <= DateTime.Now)
                return IslemSonucu.Hata("Geçmiş bir tarih veya saat için randevu alınamaz.");

            if (bitis.Date != baslangic.Date)
                return IslemSonucu.Hata("Randevu gece yarısını geçemez.");

            // --- Adım 3: Klinik o gün ve saatte açık mı? ---
            var klinikSaati = await _context.ClinicWorkingHours
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.ClinicId == hekim.ClinicId && w.Day == gun);

            if (klinikSaati == null || klinikSaati.IsClosed)
                return IslemSonucu.Hata($"Klinik {gun.TurkceAd()} günleri kapalı.");

            if (bas < klinikSaati.OpenTime || bit > klinikSaati.CloseTime)
                return IslemSonucu.Hata(
                    $"Klinik {gun.TurkceAd()} günü {klinikSaati.OpenTime:HH:mm}–{klinikSaati.CloseTime:HH:mm} arası açık. " +
                    $"Seçilen randevu ({bas:HH:mm}–{bit:HH:mm}) bu saatlerin dışında kalıyor.");

            // --- Adım 4: Hekim o saatte müsait mi? ---
            // Randevunun TAMAMI (başlangıç ve bitiş) hekimin müsaitlik aralıklarından birinin içinde olmalı
            var gunlukMusaitlik = hekim.Availabilities.Where(a => a.Day == gun).OrderBy(a => a.StartTime).ToList();
            if (!gunlukMusaitlik.Any())
                return IslemSonucu.Hata($"{hekimAdi} {gun.TurkceAd()} günleri çalışmıyor.");

            if (!gunlukMusaitlik.Any(a => a.StartTime <= bas && bit <= a.EndTime))
            {
                var araliklar = string.Join(", ", gunlukMusaitlik.Select(a => $"{a.StartTime:HH:mm}–{a.EndTime:HH:mm}"));
                return IslemSonucu.Hata(
                    $"{hekimAdi} {gun.TurkceAd()} günü yalnızca {araliklar} arası müsait. " +
                    $"Seçilen randevu ({bas:HH:mm}–{bit:HH:mm}, {hizmet.DurationMinutes} dk) bu aralığa sığmıyor.");
            }

            // --- Adım 5: Hekimin başka bir randevusuyla çakışıyor mu? ---
            // İki zaman aralığı çakışır  <=>  yeniBaşlangıç < eskiBitiş  VE  eskiBaşlangıç < yeniBitiş
            // Sadece Bekliyor ve Onaylandı randevular saati doldurur (reddedilen/iptal edilen saati boşaltır)
            var cakisan = await _context.Appointments
                .AsNoTracking()
                .Where(a => a.VeterinarianId == veterinarianId
                         && (a.Status == AppointmentStatus.Bekliyor || a.Status == AppointmentStatus.Onaylandi)
                         && (haricRandevuId == null || a.Id != haricRandevuId)
                         && baslangic < a.EndTime
                         && a.StartTime < bitis)
                .OrderBy(a => a.StartTime)
                .FirstOrDefaultAsync();

            if (cakisan != null)
                return IslemSonucu.Hata(
                    $"{hekimAdi} {cakisan.StartTime:HH:mm}–{cakisan.EndTime:HH:mm} arası dolu. " +
                    "Lütfen başka bir saat veya hekim seçin.");

            // --- Adım 6 (ek kural): Aynı hayvanın aynı saatte başka bir randevusu var mı? ---
            if (petId.HasValue)
            {
                var hayvanCakisma = await _context.Appointments
                    .AsNoTracking()
                    .AnyAsync(a => a.PetId == petId.Value
                                && (a.Status == AppointmentStatus.Bekliyor || a.Status == AppointmentStatus.Onaylandi)
                                && (haricRandevuId == null || a.Id != haricRandevuId)
                                && baslangic < a.EndTime
                                && a.StartTime < bitis);
                if (hayvanCakisma)
                    return IslemSonucu.Hata("Bu evcil hayvanın aynı saatlerde başka bir randevusu var.");
            }

            return IslemSonucu.Basari("Seçilen saat uygun.");
        }

        // =====================================================================
        // 2) UYGUN SAATLERİ HESAPLAMA
        // =====================================================================
        public async Task<List<HekimUygunSaatleri>> GetAvailableSlotsAsync(int serviceId, DateOnly tarih, int? veterinarianId = null)
        {
            var sonuc = new List<HekimUygunSaatleri>();

            var hizmet = await _context.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == serviceId);
            if (hizmet == null || !hizmet.IsActive || tarih < DateOnly.FromDateTime(DateTime.Today))
                return sonuc;

            var gun = tarih.DayOfWeek;
            var gunBasi = tarih.ToDateTime(TimeOnly.MinValue);
            var sure = TimeSpan.FromMinutes(hizmet.DurationMinutes);

            // Bu hizmeti veren aktif hekimler (servis katmanını tekrar kullanıyoruz)
            var hekimler = await _vetService.GetByServiceAsync(serviceId);
            if (veterinarianId.HasValue)
                hekimler = hekimler.Where(v => v.Id == veterinarianId.Value).ToList();
            if (!hekimler.Any())
                return sonuc;

            var hekimIdleri = hekimler.Select(v => v.Id).ToList();
            var klinikIdleri = hekimler.Select(v => v.ClinicId).Distinct().ToList();

            // Kliniklerin o günkü çalışma saatleri (tek sorgu)
            var klinikSaatleri = await _context.ClinicWorkingHours
                .AsNoTracking()
                .Where(w => klinikIdleri.Contains(w.ClinicId) && w.Day == gun)
                .ToListAsync();

            // O gün bu hekimlerin dolu saatleri (tek sorgu)
            var gunSonu = gunBasi.AddDays(1);
            var doluRandevular = await _context.Appointments
                .AsNoTracking()
                .Where(a => hekimIdleri.Contains(a.VeterinarianId)
                         && (a.Status == AppointmentStatus.Bekliyor || a.Status == AppointmentStatus.Onaylandi)
                         && a.StartTime < gunSonu
                         && a.EndTime > gunBasi)
                .ToListAsync();

            foreach (var hekim in hekimler)
            {
                var klinikSaati = klinikSaatleri.FirstOrDefault(w => w.ClinicId == hekim.ClinicId);
                if (klinikSaati == null || klinikSaati.IsClosed)
                    continue; // klinik kapalı -> bu hekim için saat yok

                var bosSaatler = new List<DateTime>();
                var hekiminRandevulari = doluRandevular.Where(a => a.VeterinarianId == hekim.Id).ToList();

                foreach (var musaitlik in hekim.Availabilities.Where(a => a.Day == gun).OrderBy(a => a.StartTime))
                {
                    // Hekimin müsaitliği ile kliniğin açık olduğu saatlerin KESİŞİMİ
                    var pencereBas = musaitlik.StartTime > klinikSaati.OpenTime ? musaitlik.StartTime : klinikSaati.OpenTime;
                    var pencereBit = musaitlik.EndTime < klinikSaati.CloseTime ? musaitlik.EndTime : klinikSaati.CloseTime;

                    // Aralığı hizmet süresi kadar dilimlere böl: 09:00, 09:30, 10:00 ... (30 dk'lık hizmet için)
                    var dilimBas = gunBasi + pencereBas.ToTimeSpan();
                    var pencereSonu = gunBasi + pencereBit.ToTimeSpan();

                    while (dilimBas + sure <= pencereSonu)
                    {
                        var dilimBit = dilimBas + sure;

                        var gecmisSaat = dilimBas <= DateTime.Now;
                        var dolu = hekiminRandevulari.Any(a => dilimBas < a.EndTime && a.StartTime < dilimBit);

                        if (!gecmisSaat && !dolu)
                            bosSaatler.Add(dilimBas);

                        dilimBas = dilimBit;
                    }
                }

                if (bosSaatler.Any())
                {
                    sonuc.Add(new HekimUygunSaatleri
                    {
                        VeterinarianId = hekim.Id,
                        HekimAdi = HekimAdi(hekim),
                        BosSaatler = bosSaatler
                    });
                }
            }

            return sonuc;
        }

        // =====================================================================
        // 3) OLUŞTURMA VE DURUM DEĞİŞİKLİKLERİ
        // =====================================================================
        public async Task<IslemSonucu<Appointment>> CreateAsync(RandevuTalebi talep, string userId)
        {
            // Güvenlik: üye sadece KENDİ hayvanına randevu alabilir
            if (!await _petService.IsOwnerAsync(talep.PetId, userId))
                return IslemSonucu<Appointment>.Hata("Evcil hayvan bulunamadı.");

            // Bütün kurallar tek yerde: ValidateAsync
            var kontrol = await ValidateAsync(talep.VeterinarianId, talep.ServiceId, talep.StartTime, talep.PetId);
            if (!kontrol.Basarili)
                return IslemSonucu<Appointment>.Hata(kontrol.Mesaj);

            var hizmet = await _context.Services.AsNoTracking().FirstAsync(s => s.Id == talep.ServiceId);

            var randevu = new Appointment
            {
                PetId = talep.PetId,
                VeterinarianId = talep.VeterinarianId,
                ServiceId = talep.ServiceId,
                StartTime = talep.StartTime,
                EndTime = talep.StartTime.AddMinutes(hizmet.DurationMinutes),
                DurationMinutes = hizmet.DurationMinutes, // o anki süre kopyalanır
                Price = hizmet.Price,                     // o anki ücret kopyalanır
                Status = AppointmentStatus.Bekliyor,      // HER ZAMAN "Bekliyor" olarak oluşur
                OwnerNote = talep.OwnerNote,
                CreatedAt = DateTime.Now
            };

            _context.Appointments.Add(randevu);
            await _context.SaveChangesAsync();

            return IslemSonucu<Appointment>.Basari(randevu,
                $"Randevunuz {randevu.StartTime:dd.MM.yyyy HH:mm} için oluşturuldu ve onay bekliyor.");
        }

        public async Task<IslemSonucu> CancelAsync(int id, string userId)
        {
            var randevu = await _context.Appointments
                .Include(a => a.Pet)
                .FirstOrDefaultAsync(a => a.Id == id && a.Pet!.OwnerId == userId); // sadece kendi randevusu
            if (randevu == null)
                return IslemSonucu.Hata("Randevu bulunamadı.");

            if (randevu.Status != AppointmentStatus.Bekliyor && randevu.Status != AppointmentStatus.Onaylandi)
                return IslemSonucu.Hata($"'{randevu.Status.GorunenAd()}' durumundaki bir randevu iptal edilemez.");

            if (randevu.StartTime <= DateTime.Now)
                return IslemSonucu.Hata("Saati geçmiş bir randevu iptal edilemez.");

            randevu.Status = AppointmentStatus.IptalEdildi;
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari("Randevunuz iptal edildi.");
        }

        public async Task<IslemSonucu> ApproveAsync(int id, string userId, bool admin)
        {
            var randevu = await PersonelIcinGetirAsync(id, userId, admin);
            if (randevu == null)
                return IslemSonucu.Hata("Randevu bulunamadı.");

            if (randevu.Status != AppointmentStatus.Bekliyor)
                return IslemSonucu.Hata("Sadece 'Bekliyor' durumundaki randevular onaylanabilir.");

            if (randevu.StartTime <= DateTime.Now)
                return IslemSonucu.Hata("Saati geçmiş bir randevu onaylanamaz.");

            randevu.Status = AppointmentStatus.Onaylandi;
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari($"{randevu.Pet?.Name} için {randevu.StartTime:dd.MM.yyyy HH:mm} randevusu onaylandı.");
        }

        public async Task<IslemSonucu> RejectAsync(int id, string? neden, string userId, bool admin)
        {
            var randevu = await PersonelIcinGetirAsync(id, userId, admin);
            if (randevu == null)
                return IslemSonucu.Hata("Randevu bulunamadı.");

            if (randevu.Status != AppointmentStatus.Bekliyor)
                return IslemSonucu.Hata("Sadece 'Bekliyor' durumundaki randevular reddedilebilir.");

            randevu.Status = AppointmentStatus.Reddedildi;   // saat tekrar boşa çıkar
            randevu.RejectionReason = string.IsNullOrWhiteSpace(neden) ? null : neden.Trim();
            await _context.SaveChangesAsync();
            return IslemSonucu.Basari("Randevu reddedildi.");
        }

        public async Task<IslemSonucu> CompleteAsync(int id, ExaminationNote not, string userId, bool admin)
        {
            var randevu = await PersonelIcinGetirAsync(id, userId, admin);
            if (randevu == null)
                return IslemSonucu.Hata("Randevu bulunamadı.");

            if (randevu.Status != AppointmentStatus.Onaylandi)
                return IslemSonucu.Hata("Sadece onaylanmış randevular tamamlanabilir.");

            if (randevu.StartTime.Date > DateTime.Today)
                return IslemSonucu.Hata("Henüz günü gelmemiş bir randevu tamamlanamaz.");

            if (string.IsNullOrWhiteSpace(not.Diagnosis))
                return IslemSonucu.Hata("Muayene notunda teşhis zorunludur.");

            randevu.ExaminationNote = new ExaminationNote
            {
                Complaint = not.Complaint,
                Diagnosis = not.Diagnosis,
                Treatment = not.Treatment,
                Notes = not.Notes,
                CreatedAt = DateTime.Now
            };
            randevu.Status = AppointmentStatus.Tamamlandi;

            await _context.SaveChangesAsync();
            return IslemSonucu.Basari("Randevu tamamlandı ve muayene notu kaydedildi.");
        }

        // =====================================================================
        // 4) SORGULAR
        // =====================================================================
        public Task<List<Appointment>> GetForOwnerAsync(string userId, bool sadeceYaklasan = false)
        {
            var sorgu = DetayliSorgu().Where(a => a.Pet!.OwnerId == userId); // sadece kendi hayvanlarının randevuları

            if (sadeceYaklasan)
                sorgu = YaklasanlariFiltrele(sorgu).OrderBy(a => a.StartTime);
            else
                sorgu = sorgu.OrderByDescending(a => a.StartTime);

            return sorgu.ToListAsync();
        }

        public Task<List<Appointment>> GetForVetAsync(int veterinarianId, bool sadeceYaklasan = false)
        {
            var sorgu = DetayliSorgu().Where(a => a.VeterinarianId == veterinarianId);

            if (sadeceYaklasan)
                sorgu = YaklasanlariFiltrele(sorgu).OrderBy(a => a.StartTime);
            else
                sorgu = sorgu.OrderByDescending(a => a.StartTime);

            return sorgu.ToListAsync();
        }

        public Task<List<Appointment>> GetAllAsync(AppointmentStatus? durum = null)
        {
            var sorgu = DetayliSorgu();
            if (durum.HasValue)
                sorgu = sorgu.Where(a => a.Status == durum.Value);

            return sorgu.OrderByDescending(a => a.StartTime).ToListAsync();
        }

        public Task<Appointment?> GetByIdForOwnerAsync(int id, string userId)
        {
            return DetayliSorgu().FirstOrDefaultAsync(a => a.Id == id && a.Pet!.OwnerId == userId);
        }

        public async Task<Appointment?> GetByIdForStaffAsync(int id, string userId, bool admin)
        {
            var randevu = await DetayliSorgu().FirstOrDefaultAsync(a => a.Id == id);
            if (randevu == null) return null;
            if (admin) return randevu;
            return randevu.Veterinarian?.UserId == userId ? randevu : null; // veteriner sadece kendi randevusunu görür
        }

        // =====================================================================
        // Yardımcılar
        // =====================================================================

        // Ekranlarda gereken tüm ilişkili verilerle randevu sorgusu
        private IQueryable<Appointment> DetayliSorgu()
        {
            return _context.Appointments
                .AsNoTracking()
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Include(a => a.Veterinarian)
                .Include(a => a.ExaminationNote);
        }

        // Bugünden itibaren, saati dolduran (Bekliyor/Onaylandı) randevular
        private static IQueryable<Appointment> YaklasanlariFiltrele(IQueryable<Appointment> sorgu)
        {
            var bugun = DateTime.Today;
            return sorgu.Where(a => a.StartTime >= bugun
                                 && (a.Status == AppointmentStatus.Bekliyor || a.Status == AppointmentStatus.Onaylandi));
        }

        // Onay/red/tamamlama için randevuyu getirir.
        // YETKİ: Admin her randevuya erişir; veteriner yalnızca KENDİ randevusuna erişir, değilse null döner.
        private async Task<Appointment?> PersonelIcinGetirAsync(int id, string userId, bool admin)
        {
            var randevu = await _context.Appointments
                .Include(a => a.Veterinarian)
                .Include(a => a.Pet)
                .Include(a => a.ExaminationNote)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (randevu == null) return null;
            if (admin) return randevu;
            return randevu.Veterinarian?.UserId == userId ? randevu : null;
        }

        private static string HekimAdi(Veterinarian hekim) => $"{hekim.Title} {hekim.FullName}".Trim();
    }
}
