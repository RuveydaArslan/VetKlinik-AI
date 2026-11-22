using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Models;

namespace VetKlinik.Controllers
{
    // Üye (hayvan sahibi) paneli: kendi hayvanları, yaklaşan aşıları ve randevuları
    [Authorize(Roles = Roles.Uye)] // sadece Üye rolündekiler girebilir
    public class PanelController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PanelController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ÖNEMLİ: sadece giriş yapan kullanıcının (OwnerId) hayvanları getirilir.
            // Başka üyelerin hayvanları bu sorguya hiçbir zaman girmez.
            var hayvanlar = await _context.Pets
                .Where(p => p.OwnerId == userId)
                .Include(p => p.Vaccinations)
                .OrderBy(p => p.Name)
                .ToListAsync();

            var yaklasanRandevular = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Include(a => a.Veterinarian)
                .Where(a => a.Pet!.OwnerId == userId
                         && a.StartTime >= DateTime.Today
                         && (a.Status == AppointmentStatus.Bekliyor || a.Status == AppointmentStatus.Onaylandi))
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            ViewBag.YaklasanRandevular = yaklasanRandevular;
            return View(hayvanlar);
        }
    }
}
