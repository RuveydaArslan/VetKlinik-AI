using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Models;

namespace VetKlinik.Data
{
    // Uygulama her açıldığında çalışır; eksik olan rolleri ve admin hesabını oluşturur.
    // Kayıt zaten varsa tekrar oluşturmaz, bu yüzden defalarca çalışması sorun değildir.
    public static class SeedData
    {
        // Proje metninde istenen admin bilgileri: ogrencinumarasi@sakarya.edu.tr / sau
        private const string AdminEmail = "B241210100@sakarya.edu.tr"; 
        private const string AdminSifre = "sau";

        public static async Task InitializeAsync(IServiceProvider services)
        {
            // Bekleyen migration varsa veritabanına uygula
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // 1) Rolleri oluştur (Admin, Veteriner, Uye)
            foreach (var rol in Roles.Tumu)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    var sonuc = await roleManager.CreateAsync(new IdentityRole(rol));
                    BasariliMi(sonuc, $"'{rol}' rolü oluşturma");
                }
            }

            // 2) Admin kullanıcısını oluştur
            var admin = await userManager.FindByEmailAsync(AdminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = AdminEmail,
                    Email = AdminEmail,
                    FullName = "Sistem Yöneticisi",
                    EmailConfirmed = true
                };

                var sonuc = await userManager.CreateAsync(admin, AdminSifre);
                BasariliMi(sonuc, "Admin kullanıcısı oluşturma");
            }

            // 3) Admin kullanıcısını Admin rolüne ekle
            if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
            {
                var sonuc = await userManager.AddToRoleAsync(admin, Roles.Admin);
                BasariliMi(sonuc, "Admin rolü atama");
            }
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
