using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Models;
using VetKlinik.Services;

namespace VetKlinik.Controllers
{
    // Üye (hayvan sahibi) paneli: kendi hayvanları, yaklaşan aşıları ve randevuları
    [Authorize(Roles = Roles.Uye)] // sadece Üye rolündekiler girebilir
    public class PanelController : Controller
    {
        private readonly IPetService _petService;
        private readonly ApplicationDbContext _context; // randevu sorgusu Gün 7'de AppointmentService'e taşınacak

        public PanelController(IPetService petService, ApplicationDbContext context)
        {
            _petService = petService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Servis sadece giriş yapan kullanıcının (OwnerId) hayvanlarını döndürür
            var hayvanlar = await _petService.GetByOwnerAsync(userId);

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
