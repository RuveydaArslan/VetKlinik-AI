using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Models;

namespace VetKlinik.Areas.Admin.Controllers
{
    // Admin paneli ana sayfası: sistemdeki kayıtların özetini gösterir
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)] // sadece Admin rolündekiler girebilir
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var bugun = DateTime.Today;

            ViewBag.KlinikSayisi = await _context.Clinics.CountAsync();
            ViewBag.HizmetSayisi = await _context.Services.CountAsync();
            ViewBag.VeterinerSayisi = await _context.Veterinarians.CountAsync();
            ViewBag.UyeSayisi = (await _userManager.GetUsersInRoleAsync(Roles.Uye)).Count;
            ViewBag.HayvanSayisi = await _context.Pets.CountAsync();
            ViewBag.BekleyenRandevu = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Bekliyor);
            ViewBag.BugunkuRandevu = await _context.Appointments
                .CountAsync(a => a.StartTime >= bugun && a.StartTime < bugun.AddDays(1));

            // Son oluşturulan 5 randevu
            var sonRandevular = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                .Include(a => a.Service)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(sonRandevular);
        }
    }
}
