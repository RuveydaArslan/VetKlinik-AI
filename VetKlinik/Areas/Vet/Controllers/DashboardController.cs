using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Models;

namespace VetKlinik.Areas.Vet.Controllers
{
    // Veteriner paneli ana sayfası: giriş yapan hekimin kendi bilgileri ve randevuları
    [Area("Vet")]
    [Authorize(Roles = Roles.Veteriner)] // sadece Veteriner rolündekiler girebilir
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Giriş yapan kullanıcının Id'si
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Bu kullanıcı hesabına bağlı hekim kaydı
            var hekim = await _context.Veterinarians
                .Include(v => v.Clinic)
                .Include(v => v.Specialties)
                .Include(v => v.Availabilities)
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (hekim == null)
            {
                // Veteriner rolünde ama henüz bir hekim kaydına bağlanmamış hesap
                return View("HekimKaydiYok");
            }

            // Yalnızca BU hekimin bugünden itibaren aktif (bekleyen/onaylı) randevuları
            var yaklasanRandevular = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Where(a => a.VeterinarianId == hekim.Id
                         && a.StartTime >= DateTime.Today
                         && (a.Status == AppointmentStatus.Bekliyor || a.Status == AppointmentStatus.Onaylandi))
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            ViewBag.Hekim = hekim;
            return View(yaklasanRandevular);
        }
    }
}
