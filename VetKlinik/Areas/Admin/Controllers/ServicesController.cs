using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetKlinik.Data;
using VetKlinik.Models;
using VetKlinik.Services;

namespace VetKlinik.Areas.Admin.Controllers
{
    // Admin paneli: Hizmet CRUD (muayene, aşı, kısırlaştırma ...; süre ve ücretiyle)
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class ServicesController : Controller
    {
        private readonly IServiceCatalogService _hizmetService;
        private readonly IClinicService _clinicService;

        public ServicesController(IServiceCatalogService hizmetService, IClinicService clinicService)
        {
            _hizmetService = hizmetService;
            _clinicService = clinicService;
        }

        // GET: /Admin/Services
        public async Task<IActionResult> Index()
        {
            return View(await _hizmetService.GetAllAsync());
        }

        // GET: /Admin/Services/Create   (klinik detayından gelinirse klinik önceden seçili olur)
        public async Task<IActionResult> Create(int? clinicId)
        {
            await KlinikListesiHazirlaAsync(clinicId);
            return View(new Service { ClinicId = clinicId ?? 0, DurationMinutes = 30, IsActive = true });
        }

        // POST: /Admin/Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClinicId,Name,Description,DurationMinutes,Price,IsActive")] Service hizmet)
        {
            if (ModelState.IsValid)
            {
                var sonuc = await _hizmetService.CreateAsync(hizmet);
                if (sonuc.Basarili)
                {
                    TempData["Basari"] = sonuc.Mesaj;
                    return RedirectToAction(nameof(Index));
                }
                // Servis kuralı ihlali (ör. aynı isimde hizmet) -> formda göster
                ModelState.AddModelError(string.Empty, sonuc.Mesaj);
            }

            await KlinikListesiHazirlaAsync(hizmet.ClinicId);
            return View(hizmet);
        }

        // GET: /Admin/Services/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var hizmet = await _hizmetService.GetByIdAsync(id);
            if (hizmet == null)
                return NotFound();

            await KlinikListesiHazirlaAsync(hizmet.ClinicId);
            return View(hizmet);
        }

        // POST: /Admin/Services/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClinicId,Name,Description,DurationMinutes,Price,IsActive")] Service hizmet)
        {
            if (id != hizmet.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var sonuc = await _hizmetService.UpdateAsync(hizmet);
                if (sonuc.Basarili)
                {
                    TempData["Basari"] = sonuc.Mesaj;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, sonuc.Mesaj);
            }

            await KlinikListesiHazirlaAsync(hizmet.ClinicId);
            return View(hizmet);
        }

        // GET: /Admin/Services/Delete/5  -> onay sayfası
        public async Task<IActionResult> Delete(int id)
        {
            var hizmet = await _hizmetService.GetByIdAsync(id);
            if (hizmet == null)
                return NotFound();

            return View(hizmet);
        }

        // POST: /Admin/Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sonuc = await _hizmetService.DeleteAsync(id);
            TempData[sonuc.Basarili ? "Basari" : "Hata"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Index));
        }

        // Formdaki "Klinik" açılır listesi
        private async Task KlinikListesiHazirlaAsync(int? seciliKlinikId)
        {
            ViewBag.Klinikler = new SelectList(await _clinicService.GetAllAsync(), "Id", "Name", seciliKlinikId);
        }
    }
}
