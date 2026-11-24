using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Models;

namespace VetKlinik.Data
{
    // Uygulama her açıldığında çalışır.
    // 1) Rolleri ve admin hesabını, 2) veritabanı boşsa örnek verileri oluşturur.
    // Kayıtlar zaten varsa tekrar oluşturmaz, bu yüzden defalarca çalışması sorun değildir.
    public static class SeedData
    {
        // Proje metninde istenen admin bilgileri: ogrencinumarasi@sakarya.edu.tr / sau
        private const string AdminEmail = "B241210100@sakarya.edu.tr";
        private const string AdminSifre = "sau";

        // Örnek veteriner ve üye hesaplarının ortak şifresi (sadece test/sunum için)
        private const string OrnekSifre = "123";

        public static async Task InitializeAsync(IServiceProvider services)
        {
            // Bekleyen migration varsa veritabanına uygula
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            await RolleriOlusturAsync(roleManager);
            await KullaniciOlusturAsync(userManager, AdminEmail, AdminSifre, "Sistem Yöneticisi", Roles.Admin);

            // Klinik tablosu boşsa örnek verileri ekle (sadece ilk çalıştırmada)
            if (!await context.Clinics.AnyAsync())
            {
                await OrnekVerileriOlusturAsync(context, userManager);
            }
        }

        // ---------------------------------------------------------------
        // Roller
        // ---------------------------------------------------------------
        private static async Task RolleriOlusturAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var rol in Roles.Tumu)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    var sonuc = await roleManager.CreateAsync(new IdentityRole(rol));
                    BasariliMi(sonuc, $"'{rol}' rolü oluşturma");
                }
            }
        }

        // Kullanıcı yoksa oluşturur, ilgili role ekler ve kullanıcıyı döndürür
        private static async Task<ApplicationUser> KullaniciOlusturAsync(
            UserManager<ApplicationUser> userManager, string email, string sifre, string adSoyad, string rol)
        {
            var kullanici = await userManager.FindByEmailAsync(email);
            if (kullanici == null)
            {
                kullanici = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = adSoyad,
                    EmailConfirmed = true
                };
                BasariliMi(await userManager.CreateAsync(kullanici, sifre), $"'{email}' kullanıcısı oluşturma");
            }

            if (!await userManager.IsInRoleAsync(kullanici, rol))
            {
                BasariliMi(await userManager.AddToRoleAsync(kullanici, rol), $"'{email}' kullanıcısına '{rol}' rolü atama");
            }

            return kullanici;
        }

        // ---------------------------------------------------------------
        // Örnek veriler: klinik, hizmetler, uzmanlıklar, hekimler, üye, evcil hayvanlar, aşılar, randevular
        // ---------------------------------------------------------------
        private static async Task OrnekVerileriOlusturAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // ÖNEMLİ: Kullanıcı hesapları EN BAŞTA oluşturulmalı.
            // UserManager.CreateAsync aynı DbContext üzerinde kendi SaveChanges'ını çağırır;
            // o anda bekleyen (eklenmiş) klinik/hizmet kayıtları varsa onları da yarım şekilde kaydeder.
            var ayseHesap   = await KullaniciOlusturAsync(userManager, "ayse.yilmaz@vetklinik.com", OrnekSifre, "Ayşe Yılmaz", Roles.Veteriner);
            var mehmetHesap = await KullaniciOlusturAsync(userManager, "mehmet.kaya@vetklinik.com", OrnekSifre, "Mehmet Kaya", Roles.Veteriner);
            var zeynepHesap = await KullaniciOlusturAsync(userManager, "zeynep.demir@vetklinik.com", OrnekSifre, "Zeynep Demir", Roles.Veteriner);
            var uye         = await KullaniciOlusturAsync(userManager, "uye@vetklinik.com", OrnekSifre, "Elif Öztürk", Roles.Uye);

            // ----- Klinik ve çalışma saatleri -----
            var klinik = new Clinic
            {
                Name = "Pati Dostu Veteriner Kliniği",
                Address = "Kemalpaşa Mah. Üniversite Cad. No:12, Serdivan / Sakarya",
                Phone = "0264 555 12 34",
                Email = "iletisim@patidostu.com",
                Description = "Kedi, köpek ve egzotik hayvanlar için muayene, aşı, cerrahi ve bakım hizmetleri."
            };

            // Hafta içi 09:00-18:00, Cumartesi 10:00-16:00, Pazar kapalı
            foreach (var gun in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday })
            {
                klinik.WorkingHours.Add(new ClinicWorkingHour { Day = gun, OpenTime = new TimeOnly(9, 0), CloseTime = new TimeOnly(18, 0) });
            }
            klinik.WorkingHours.Add(new ClinicWorkingHour { Day = DayOfWeek.Saturday, OpenTime = new TimeOnly(10, 0), CloseTime = new TimeOnly(16, 0) });
            klinik.WorkingHours.Add(new ClinicWorkingHour { Day = DayOfWeek.Sunday, IsClosed = true });

            // ----- Hizmetler (süre ve ücretleriyle) -----
            var muayene   = new Service { Clinic = klinik, Name = "Genel Muayene",        DurationMinutes = 30,  Price = 500m,  Description = "Genel sağlık kontrolü ve fiziksel muayene." };
            var asi       = new Service { Clinic = klinik, Name = "Aşı Uygulaması",       DurationMinutes = 15,  Price = 350m,  Description = "Karma, kuduz, lösemi vb. aşıların uygulanması." };
            var kisir     = new Service { Clinic = klinik, Name = "Kısırlaştırma",        DurationMinutes = 120, Price = 3500m, Description = "Genel anestezi altında kısırlaştırma operasyonu." };
            var dis       = new Service { Clinic = klinik, Name = "Diş Temizliği",        DurationMinutes = 60,  Price = 1500m, Description = "Diş taşı temizliği ve ağız bakımı." };
            var tiras     = new Service { Clinic = klinik, Name = "Tıraş ve Bakım",       DurationMinutes = 60,  Price = 750m,  Description = "Tüy kesimi, banyo, tırnak ve kulak bakımı." };
            var dermato   = new Service { Clinic = klinik, Name = "Dermatolojik Muayene", DurationMinutes = 45,  Price = 800m,  Description = "Deri ve tüy hastalıklarının muayenesi." };
            context.Services.AddRange(muayene, asi, kisir, dis, tiras, dermato);

            // ----- Uzmanlık alanları -----
            var kucukHayvan = new Specialty { Name = "Küçük Hayvan Hekimliği", Description = "Kedi ve köpeklerin genel sağlığı." };
            var egzotik     = new Specialty { Name = "Egzotik Hayvan Hekimliği", Description = "Kuş, tavşan, sürüngen vb. hayvanlar." };
            var cerrahi     = new Specialty { Name = "Cerrahi", Description = "Operasyon ve cerrahi müdahaleler." };
            var dermatoloji = new Specialty { Name = "Dermatoloji", Description = "Deri ve tüy hastalıkları." };
            context.Specialties.AddRange(kucukHayvan, egzotik, cerrahi, dermatoloji);

            // ----- Veteriner hekimler -----
            var ayse = new Veterinarian
            {
                Clinic = klinik, FullName = "Ayşe Yılmaz", Title = "Vet. Hek.", Phone = "0532 111 22 33", UserId = ayseHesap.Id,
                Biography = "Küçük hayvan hekimliği ve dermatoloji alanında 8 yıllık deneyim.",
                Specialties = { kucukHayvan, dermatoloji },
                Services = { muayene, asi, dermato, tiras }
            };
            // Hafta içi her gün 09:00-17:00
            foreach (var gun in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday })
            {
                ayse.Availabilities.Add(new VetAvailability { Day = gun, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0) });
            }

            var mehmet = new Veterinarian
            {
                Clinic = klinik, FullName = "Mehmet Kaya", Title = "Op. Dr.", Phone = "0533 222 33 44", UserId = mehmetHesap.Id,
                Biography = "Yumuşak doku cerrahisi ve kısırlaştırma operasyonlarında uzman.",
                Specialties = { cerrahi, kucukHayvan },
                Services = { muayene, kisir, dis }
            };
            // Pazartesi, Çarşamba, Cuma 10:00-18:00; Cumartesi 10:00-14:00
            foreach (var gun in new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday })
            {
                mehmet.Availabilities.Add(new VetAvailability { Day = gun, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(18, 0) });
            }
            mehmet.Availabilities.Add(new VetAvailability { Day = DayOfWeek.Saturday, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(14, 0) });

            var zeynep = new Veterinarian
            {
                Clinic = klinik, FullName = "Zeynep Demir", Title = "Vet. Hek.", Phone = "0534 333 44 55", UserId = zeynepHesap.Id,
                Biography = "Kuş, tavşan ve sürüngen gibi egzotik hayvanlarla çalışıyor.",
                Specialties = { egzotik, kucukHayvan },
                Services = { muayene, asi, tiras }
            };
            // Salı, Perşembe 09:00-17:00; Cumartesi 10:00-16:00
            foreach (var gun in new[] { DayOfWeek.Tuesday, DayOfWeek.Thursday })
            {
                zeynep.Availabilities.Add(new VetAvailability { Day = gun, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0) });
            }
            zeynep.Availabilities.Add(new VetAvailability { Day = DayOfWeek.Saturday, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(16, 0) });

            context.Clinics.Add(klinik);
            context.Veterinarians.AddRange(ayse, mehmet, zeynep);
            // (Tüm veriler en sonda tek bir SaveChangesAsync ile birlikte kaydedilir)

            // ----- Örnek üyenin evcil hayvanları -----
            var bugun = DateOnly.FromDateTime(DateTime.Today);

            var pamuk = new Pet
            {
                OwnerId = uye.Id, Name = "Pamuk", Species = "Kedi", Breed = "Van Kedisi",
                BirthDate = bugun.AddYears(-3).AddMonths(-2), Weight = 4.2m
            };
            // Aşı tarihleri bugüne göre hesaplanıyor: biri yaklaşan, biri gecikmiş, biri ileri tarihli
            pamuk.Vaccinations.Add(new Vaccination { VaccineName = "Karma Aşı",     DateGiven = bugun.AddDays(-350), NextDoseDate = bugun.AddDays(15) });
            pamuk.Vaccinations.Add(new Vaccination { VaccineName = "Kuduz",         DateGiven = bugun.AddDays(-380), NextDoseDate = bugun.AddDays(-15), Notes = "Hatırlatma dozu gecikti." });
            pamuk.Vaccinations.Add(new Vaccination { VaccineName = "Lösemi (FeLV)", DateGiven = bugun.AddDays(-160), NextDoseDate = bugun.AddDays(205) });

            var karabas = new Pet
            {
                OwnerId = uye.Id, Name = "Karabaş", Species = "Köpek", Breed = "Kangal",
                BirthDate = bugun.AddYears(-5).AddMonths(-4), Weight = 38.5m
            };
            karabas.Vaccinations.Add(new Vaccination { VaccineName = "Kuduz", DateGiven = bugun.AddDays(-120), NextDoseDate = bugun.AddDays(245) });

            context.Pets.AddRange(pamuk, karabas);

            // ----- Örnek randevular -----
            // 1) Geçmişte tamamlanmış bir muayene (muayene notuyla)
            var gecmisGun = OncekiIsGunu(DateTime.Today.AddDays(-7));
            var gecmis = RandevuOlustur(pamuk, ayse, muayene, gecmisGun.AddHours(10), AppointmentStatus.Tamamlandi);
            gecmis.ExaminationNote = new ExaminationNote
            {
                Complaint = "İştahsızlık ve halsizlik",
                Diagnosis = "Hafif mide rahatsızlığı",
                Treatment = "3 gün diyet mama ve probiyotik takviyesi",
                Notes = "Bir hafta sonra kontrol önerildi.",
                CreatedAt = gecmis.EndTime
            };

            // 2) Gelecekte onaylanmış bir muayene (Karabaş, ~3 gün sonra)
            var onayliGun = SonrakiIsGunu(DateTime.Today.AddDays(3));
            var onayli = RandevuOlustur(karabas, ayse, muayene, onayliGun.AddHours(10), AppointmentStatus.Onaylandi);

            // 3) Gelecekte onay bekleyen bir aşı randevusu (Pamuk, ~1 hafta sonra)
            //    Seed sadece ilk çalıştırmada çalışır; bu yüzden tarihler birkaç gün ileri verildi.
            var gelecekGun = SonrakiIsGunu(DateTime.Today.AddDays(7));
            var bekleyen = RandevuOlustur(pamuk, ayse, asi, gelecekGun.AddHours(14), AppointmentStatus.Bekliyor);
            bekleyen.OwnerNote = "Kuduz aşısının hatırlatma dozu için.";

            context.Appointments.AddRange(gecmis, onayli, bekleyen);

            await context.SaveChangesAsync();
        }

        // Randevu oluştururken süre ve ücret hizmetten kopyalanır, bitiş saati hesaplanır
        private static Appointment RandevuOlustur(Pet pet, Veterinarian vet, Service hizmet, DateTime baslangic, AppointmentStatus durum)
        {
            return new Appointment
            {
                Pet = pet,
                Veterinarian = vet,
                Service = hizmet,
                StartTime = baslangic,
                EndTime = baslangic.AddMinutes(hizmet.DurationMinutes),
                DurationMinutes = hizmet.DurationMinutes,
                Price = hizmet.Price,
                Status = durum,
                CreatedAt = baslangic.AddDays(-2)
            };
        }

        // Verilen tarih hafta sonuna denk geliyorsa bir sonraki pazartesiye kaydırır
        private static DateTime SonrakiIsGunu(DateTime tarih)
        {
            while (tarih.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                tarih = tarih.AddDays(1);
            return tarih.Date;
        }

        // Verilen tarih hafta sonuna denk geliyorsa bir önceki cumaya kaydırır
        private static DateTime OncekiIsGunu(DateTime tarih)
        {
            while (tarih.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                tarih = tarih.AddDays(-1);
            return tarih.Date;
        }

        // Identity işlemi başarısızsa hataları birleştirip anlaşılır bir hata fırlatır
        private static void BasariliMi(IdentityResult sonuc, string islem)
        {
            if (!sonuc.Succeeded)
            {
                var hatalar = string.Join(", ", sonuc.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"{islem} başarısız: {hatalar}");
            }
        }
    }
}
