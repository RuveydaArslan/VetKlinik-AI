using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Models;
using VetKlinik.Services;

var builder = WebApplication.CreateBuilder(args);

// Veritabanı bağlantı dizesini appsettings.json'dan oku
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("'DefaultConnection' bağlantı dizesi bulunamadı.");

// EF Core: ApplicationDbContext SQL Server kullanacak
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity: kullanıcı tipi ApplicationUser, roller IdentityRole
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Admin şifresi "sau" (3 karakter) olduğu için şifre kurallarını gevşetiyoruz
    options.Password.RequiredLength = 3;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredUniqueChars = 1;

    // E-posta onayı olmadan giriş yapılabilsin
    options.SignIn.RequireConfirmedAccount = false;

    // Aynı e-posta ile iki hesap açılamasın
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()       // kullanıcı/rol verisini bu DbContext'te sakla
.AddDefaultTokenProviders()                             // şifre sıfırlama vb. için token üreticileri
.AddErrorDescriber<TurkceIdentityHataMesajlari>();      // Identity hata mesajlarını Türkçe göster

// Giriş çerezi ayarları: giriş yapmamış kullanıcı yetkili bir sayfaya girerse
// kendi yazdığımız AccountController sayfalarına yönlendirilir
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Servis katmanı: iş kuralları tek yerde yazılır.
// MVC controller'ları, REST API ve MCP araçları bu servisleri kullanır.
// AddScoped: her HTTP isteği için bir örnek oluşturulur (DbContext ile aynı ömür).
builder.Services.AddScoped<IClinicService, ClinicService>();
builder.Services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
builder.Services.AddScoped<ISpecialtyService, SpecialtyService>();
builder.Services.AddScoped<IVetService, VetService>();
builder.Services.AddScoped<IPetService, PetService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Uygulama açılırken rolleri ve admin hesabını oluştur (yoksa)
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // kullanıcı kim? (giriş bilgisini okur)
app.UseAuthorization();  // bu sayfaya girebilir mi? (yetki kontrolü)

app.MapStaticAssets();

// Area'lar için rota: /Admin/Dashboard, /Vet/Dashboard ...
// (default rotadan ÖNCE tanımlanmalı)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
