using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Data;
using VetKlinik.Models;
using VetKlinik.Services;

namespace VetKlinik.Areas.Admin.Controllers
{
    // Admin paneli: Klinik CRUD + çalışma saatleri
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class ClinicsController : Controller
    {
        private readonly IClinicService _clinicService;

        public ClinicsController(IClinicService clinicService)
        {
            _clinicService = clinicService;
        }

        // GET: /Admin/Clinics
        public async Task<IActionResult> Index()
        {
            return View(await _clinicService.GetAllAsync());
        }

        // GET: /Admin/Clinics/Details/5  (çalışma saatleri de bu sayfadan yönetilir)
        public async Task<IActionResult> Details(int id)
        {
            var klinik = await _clinicService.GetByIdAsync(id);
            if (klinik == null)
                return NotFound();

            return View(klinik);
        }

        // GET: /Admin/Clinics/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Clinics/Create
        // [Bind]: sadece formda olması gereken alanlar modele aktarılır (over-posting saldırısına karşı)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Address,Phone,Email,Description")] Clinic klinik)
        {
            if (!ModelState.IsValid)
                return View(klinik);

            var sonuc = await _clinicService.CreateAsync(klinik);
            TempData["Basari"] = sonuc.Mesaj + " Şimdi çalışma saatlerini ekleyebilirsiniz.";
            return RedirectToAction(nameof(Details), new { id = klinik.Id });
        }

        // GET: /Admin/Clinics/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var klinik = await _clinicService.GetByIdAsync(id);
            if (klinik == null)
                return NotFound();

            return View(klinik);
        }

        // POST: /Admin/Clinics/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Phone,Email,Description")] Clinic klinik)
        {
            if (id != klinik.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(klinik);

            var sonuc = await _clinicService.UpdateAsync(klinik);
            if (!sonuc.Basarili)
            {
                ModelState.AddModelError(string.Empty, sonuc.Mesaj);
                return View(klinik);
            }

            TempData["Basari"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /Admin/Clinics/Delete/5  -> silme ONAY sayfası
        public async Task<IActionResult> Delete(int id)
        {
            var klinik = await _clinicService.GetByIdAsync(id);
            if (klinik == null)
                return NotFound();

            return View(klinik);
        }

        // POST: /Admin/Clinics/Delete/5  -> onaylandıktan sonra gerçekten siler
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sonuc = await _clinicService.DeleteAsync(id);
            TempData[sonuc.Basarili ? "Basari" : "Hata"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Clinics/SaveWorkingHour  (gün için ekle veya güncelle)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveWorkingHour([Bind("ClinicId,Day,OpenTime,CloseTime,IsClosed")] ClinicWorkingHour saat)
        {
            // Kurallar (kapanış > açılış vb.) servis içinde kontrol ediliyor
            var sonuc = await _clinicService.SaveWorkingHourAsync(saat);
            TempData[sonuc.Basarili ? "Basari" : "Hata"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Details), new { id = saat.ClinicId });
        }

        // POST: /Admin/Clinics/DeleteWorkingHour
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteWorkingHour(int id, int clinicId)
        {
            var sonuc = await _clinicService.DeleteWorkingHourAsync(id);
            TempData[sonuc.Basarili ? "Basari" : "Hata"] = sonuc.Mesaj;
            return RedirectToAction(nameof(Details), new { id = clinicId });
        }
    }
}
