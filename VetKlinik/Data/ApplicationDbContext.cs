using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Models;

namespace VetKlinik.Data
{
    // Uygulamanın veritabanı bağlamı.
    // IdentityDbContext<ApplicationUser> sayesinde kullanıcı, rol vb. Identity tabloları
    // (AspNetUsers, AspNetRoles, AspNetUserRoles...) otomatik olarak oluşturulur.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Projenin kendi tabloları (Klinik, Hizmet, Veteriner...) Gün 3-4'te buraya DbSet olarak eklenecek.
    }
}
