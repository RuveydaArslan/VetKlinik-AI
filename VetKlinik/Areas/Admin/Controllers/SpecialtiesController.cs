using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Data;
using VetKlinik.Models;
using VetKlinik.Services;

namespace VetKlinik.Areas.Admin.Controllers
{
    // Admin paneli: Uzmanlık alanı CRUD (küçük hayvan, egzotik, cerrahi, dermatoloji ...)
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class SpecialtiesController : Controller
    {
        private readonly ISpecialtyService _uzmanlikService;

        public SpecialtiesController(ISpecialtyService uzmanlikService)
        {
            _uzmanlikService = uzmanlikService;
        }

        // GET: /Admin/Specialties
        public async Task<IActionResult> Index()
        {
            return View(await _uzmanlikService.GetAllAsync());
        }

        // GET: /Admin/Specialties/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Specialties/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Specialty uzmanlik)
        {
            if (ModelState.IsValid)
            {
                var sonuc = await _uzmanlikService.CreateAsync(uzmanlik);
                if (sonuc.Basarili)
                {
                    TempData["Basari"] = sonuc.Mesaj;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(nameof(Specialty.Name), sonuc.Mesaj);
            }
            return View(uzmanlik);
        }

        // GET: /Admin/Specialties/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var uzmanlik = await _uzmanlikService.GetByIdAsync(id);
            if (uzmanlik == null)
                return NotFound();

            return View(uzmanlik);
        }

        // POST: /Admin/Specialties/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Specialty uzmanlik)
        {
            if (id != uzmanlik.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var sonuc = await _uzmanlikService.UpdateAsync(uzmanlik);
                if (sonuc.Basarili)
                {
                    TempData["Basari"] = sonuc.Mesaj;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(nameof(Specialty.Name), sonuc.Mesaj);
            }
            return View(uzmanlik);
        }

        // GET: /Admin/Specialties/Delete/5  -> onay sayfası
        public async Task<IActionResult> Delete(int id)
        {
            var uzmanlik = await _uzmanlikService.GetByIdAsync(id);
            if (uzmanlik == null)
                return NotFound();

            return View(uzmanlik);
        }

        // POST: /Admin/Specialties/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sonuc = await _uzmanlikService.DeleteAsync(id);
            TempData[sonuc.Basarili ? "Basari" : "Hata"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Index));
        }
    }
}
