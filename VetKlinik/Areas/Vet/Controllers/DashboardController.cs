using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Data;
using VetKlinik.Services;

namespace VetKlinik.Areas.Vet.Controllers
{
    // Veteriner paneli ana sayfası: giriş yapan hekimin kendi bilgileri ve randevuları
    [Area("Vet")]
    [Authorize(Roles = Roles.Veteriner)] // sadece Veteriner rolündekiler girebilir
    public class DashboardController : Controller
    {
        private readonly IVetService _vetService;
        private readonly IAppointmentService _appointmentService;

        public DashboardController(IVetService vetService, IAppointmentService appointmentService)
        {
            _vetService = vetService;
            _appointmentService = appointmentService;
        }

        public async Task<IActionResult> Index()
        {
            // Giriş yapan kullanıcının Id'si
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Bu kullanıcı hesabına bağlı hekim kaydı
            var hekim = await _vetService.GetByUserIdAsync(userId);
            if (hekim == null)
            {
                // Veteriner rolünde ama henüz bir hekim kaydına bağlanmamış hesap
                return View("HekimKaydiYok");
            }

            // Yalnızca BU hekimin bugünden itibaren aktif (bekleyen/onaylı) randevuları
            var yaklasanRandevular = await _appointmentService.GetForVetAsync(hekim.Id, sadeceYaklasan: true);

            ViewBag.Hekim = hekim;
            return View(yaklasanRandevular);
        }
    }
}
