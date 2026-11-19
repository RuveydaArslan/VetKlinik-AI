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

        // Her DbSet veritabanında bir tabloya karşılık gelir
        public DbSet<Clinic> Clinics => Set<Clinic>();
        public DbSet<ClinicWorkingHour> ClinicWorkingHours => Set<ClinicWorkingHour>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<Specialty> Specialties => Set<Specialty>();
        public DbSet<Veterinarian> Veterinarians => Set<Veterinarian>();
        public DbSet<VetAvailability> VetAvailabilities => Set<VetAvailability>();

        // Attribute'larla ifade edilemeyen ayarlar (ilişkiler, silme davranışları, indeksler) burada yapılır
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Identity tablolarının ayarları için mutlaka önce base çağrılmalı
            base.OnModelCreating(builder);

            // ---------- Klinik ----------
            // Klinik silinirse çalışma saatleri de silinsin
            builder.Entity<ClinicWorkingHour>()
                .HasOne(w => w.Clinic)
                .WithMany(c => c.WorkingHours)
                .HasForeignKey(w => w.ClinicId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bir kliniğin aynı gün için tek bir çalışma saati kaydı olabilir
            builder.Entity<ClinicWorkingHour>()
                .HasIndex(w => new { w.ClinicId, w.Day })
                .IsUnique();

            // ---------- Hizmet ----------
            // Para alanı: toplam 10 basamak, virgülden sonra 2 basamak
            builder.Entity<Service>()
                .Property(s => s.Price)
                .HasPrecision(10, 2);

            // Hizmeti veya hekimi olan bir klinik yanlışlıkla silinemesin
            builder.Entity<Service>()
                .HasOne(s => s.Clinic)
                .WithMany(c => c.Services)
                .HasForeignKey(s => s.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- Uzmanlık ----------
            builder.Entity<Specialty>()
                .HasIndex(s => s.Name)
                .IsUnique();

            // ---------- Veteriner ----------
            builder.Entity<Veterinarian>()
                .HasOne(v => v.Clinic)
                .WithMany(c => c.Veterinarians)
                .HasForeignKey(v => v.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            // Kullanıcı hesabı silinirse hekim kaydı kalsın, sadece bağlantı boşalsın
            builder.Entity<Veterinarian>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Bir kullanıcı hesabı en fazla bir hekime bağlanabilir
            builder.Entity<Veterinarian>()
                .HasIndex(v => v.UserId)
                .IsUnique();

            // Çoka-çok: Hekim <-> Uzmanlık (ara tablo: VeterinarianSpecialties)
            builder.Entity<Veterinarian>()
                .HasMany(v => v.Specialties)
                .WithMany(s => s.Veterinarians)
                .UsingEntity(j => j.ToTable("VeterinarianSpecialties"));

            // Çoka-çok: Hekim <-> Hizmet (ara tablo: VeterinarianServices)
            builder.Entity<Veterinarian>()
                .HasMany(v => v.Services)
                .WithMany(s => s.Veterinarians)
                .UsingEntity(j => j.ToTable("VeterinarianServices"));

            // Hekim silinirse müsaitlik saatleri de silinsin
            builder.Entity<VetAvailability>()
                .HasOne(a => a.Veterinarian)
                .WithMany(v => v.Availabilities)
                .HasForeignKey(a => a.VeterinarianId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
