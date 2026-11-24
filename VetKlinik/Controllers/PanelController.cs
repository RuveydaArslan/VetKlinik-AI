using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Data;
using VetKlinik.Services;

namespace VetKlinik.Controllers
{
    // Üye (hayvan sahibi) paneli: kendi hayvanları, yaklaşan aşıları ve randevuları
    [Authorize(Roles = Roles.Uye)] // sadece Üye rolündekiler girebilir
    public class PanelController : Controller
    {
        private readonly IPetService _petService;
        private readonly IAppointmentService _appointmentService;

        public PanelController(IPetService petService, IAppointmentService appointmentService)
        {
            _petService = petService;
            _appointmentService = appointmentService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Servisler sadece giriş yapan kullanıcının verilerini döndürür
            var hayvanlar = await _petService.GetByOwnerAsync(userId);
            ViewBag.YaklasanRandevular = await _appointmentService.GetForOwnerAsync(userId, sadeceYaklasan: true);

            return View(hayvanlar);
        }
    }
}
