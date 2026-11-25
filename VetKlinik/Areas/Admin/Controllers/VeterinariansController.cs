using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetKlinik.Data;
using VetKlinik.Models;
using VetKlinik.Services;
using VetKlinik.ViewModels;

namespace VetKlinik.Areas.Admin.Controllers
{
    // Admin paneli: Veteriner hekim CRUD + uzmanlık/hizmet seçimi + müsaitlik saatleri
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class VeterinariansController : Controller
    {
        private readonly IVetService _vetService;
        private readonly IClinicService _clinicService;
        private readonly ISpecialtyService _uzmanlikService;
        private readonly IServiceCatalogService _hizmetService;
        private readonly IAppointmentService _appointmentService;

        public VeterinariansController(IVetService vetService, IClinicService clinicService,
            ISpecialtyService uzmanlikService, IServiceCatalogService hizmetService,
            IAppointmentService appointmentService)
        {
            _vetService = vetService;
            _clinicService = clinicService;
            _uzmanlikService = uzmanlikService;
            _hizmetService = hizmetService;
            _appointmentService = appointmentService;
        }

        // GET: /Admin/Veterinarians
        public async Task<IActionResult> Index()
        {
            return View(await _vetService.GetAllAsync());
        }

        // GET: /Admin/Veterinarians/Details/5  (müsaitlik saatleri de bu sayfadan yönetilir)
        public async Task<IActionResult> Details(int id)
        {
            var hekim = await _vetService.GetByIdAsync(id);
            if (hekim == null)
                return NotFound();

            ViewBag.YaklasanRandevular = await _appointmentService.GetForVetAsync(id, sadeceYaklasan: true);
            return View(hekim);
        }

        // GET: /Admin/Veterinarians/Create
        public async Task<IActionResult> Create()
        {
            var vm = new VeterinerFormViewModel { IsActive = true };
            await FormHazirlaAsync(vm);
            return View(vm);
        }

        // POST: /Admin/Veterinarians/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VeterinerFormViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var hekim = EntityyeCevir(vm);
                var sonuc = await _vetService.CreateAsync(hekim, vm.SecilenUzmanlikIdleri, vm.SecilenHizmetIdleri, YeniHesapBilgisi(vm));
                if (sonuc.Basarili)
                {
                    TempData["Basari"] = sonuc.Mesaj + " Şimdi müsaitlik saatlerini ekleyebilirsiniz.";
                    return RedirectToAction(nameof(Details), new { id = hekim.Id });
                }
                ModelState.AddModelError(string.Empty, sonuc.Mesaj); // ortadaki kutucukta gösterilir
            }

            await FormHazirlaAsync(vm);
            return View(vm);
        }

        // GET: /Admin/Veterinarians/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var hekim = await _vetService.GetByIdAsync(id);
            if (hekim == null)
                return NotFound();

            var vm = new VeterinerFormViewModel
            {
                Id = hekim.Id,
                ClinicId = hekim.ClinicId,
                FullName = hekim.FullName,
                Title = hekim.Title,
                Phone = hekim.Phone,
                Biography = hekim.Biography,
                IsActive = hekim.IsActive,
                UserId = hekim.UserId,
                SecilenUzmanlikIdleri = hekim.Specialties.Select(s => s.Id).ToList(),
                SecilenHizmetIdleri = hekim.Services.Select(s => s.Id).ToList()
            };
            await FormHazirlaAsync(vm);
            return View(vm);
        }

        // POST: /Admin/Veterinarians/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VeterinerFormViewModel vm)
        {
            if (id != vm.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var sonuc = await _vetService.UpdateAsync(EntityyeCevir(vm), vm.SecilenUzmanlikIdleri, vm.SecilenHizmetIdleri, YeniHesapBilgisi(vm));
                if (sonuc.Basarili)
                {
                    TempData["Basari"] = sonuc.Mesaj;
                    return RedirectToAction(nameof(Details), new { id });
                }
                ModelState.AddModelError(string.Empty, sonuc.Mesaj);
            }

            await FormHazirlaAsync(vm);
            return View(vm);
        }

        // GET: /Admin/Veterinarians/Delete/5  -> onay sayfası
        public async Task<IActionResult> Delete(int id)
        {
            var hekim = await _vetService.GetByIdAsync(id);
            if (hekim == null)
                return NotFound();

            return View(hekim);
        }

        // POST: /Admin/Veterinarians/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sonuc = await _vetService.DeleteAsync(id);
            TempData[sonuc.Basarili ? "Basari" : "Hata"] = sonuc.Mesaj;
            return sonuc.Basarili ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Admin/Veterinarians/AddAvailability
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAvailability([Bind("VeterinarianId,Day,StartTime,EndTime")] VetAvailability musaitlik)
        {
            // Kurallar (klinik saatleri içinde mi, çakışma var mı) VetService'te
            var sonuc = await _vetService.AddAvailabilityAsync(musaitlik);
            TempData[sonuc.Basarili ? "Basari" : "Hata"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Details), new { id = musaitlik.VeterinarianId });
        }

        // POST: /Admin/Veterinarians/DeleteAvailability
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAvailability(int id, int veterinarianId)
        {
            var sonuc = await _vetService.DeleteAvailabilityAsync(id);
            TempData[sonuc.Basarili ? "Basari" : "Hata"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Details), new { id = veterinarianId });
        }

        // ---------------- Yardımcılar ----------------

        // Formun açılır listelerini ve checkbox listelerini doldurur
        private async Task FormHazirlaAsync(VeterinerFormViewModel vm)
        {
            vm.TumUzmanliklar = await _uzmanlikService.GetAllAsync();
            vm.TumHizmetler = await _hizmetService.GetAllAsync();
            vm.Klinikler = new SelectList(await _clinicService.GetAllAsync(), "Id", "Name", vm.ClinicId);

            var hesaplar = await _vetService.GetAssignableUsersAsync(vm.Id == 0 ? null : vm.Id);
            vm.Hesaplar = new SelectList(
                hesaplar.Select(u => new { u.Id, Ad = $"{u.FullName} ({u.Email})" }), "Id", "Ad", vm.UserId);
        }

        // ViewModel -> Entity
        private static Veterinarian EntityyeCevir(VeterinerFormViewModel vm) => new()
        {
            Id = vm.Id,
            ClinicId = vm.ClinicId,
            FullName = vm.FullName,
            Title = vm.Title,
            Phone = vm.Phone,
            Biography = vm.Biography,
            IsActive = vm.IsActive,
            UserId = string.IsNullOrEmpty(vm.UserId) ? null : vm.UserId
        };

        private static YeniHesap? YeniHesapBilgisi(VeterinerFormViewModel vm) =>
            string.IsNullOrWhiteSpace(vm.YeniHesapEmail) ? null : new YeniHesap(vm.YeniHesapEmail.Trim(), vm.YeniHesapSifre!);
    }
}
