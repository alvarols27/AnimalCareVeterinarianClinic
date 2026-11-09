using AnimalCareVeterinaryClinicDBFirst.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AnimalCareVeterinaryClinicDBFirst.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class VeterinariansController : Controller
    {
        private readonly AnimalCareContext _context;

        public VeterinariansController(AnimalCareContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return _context.Veterinarians != null
                ? View(await _context.Veterinarians.ToListAsync())
                : Problem("Entity set 'AnimalCareContext.Veterinarians' is null.");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Veterinarians == null) return NotFound();

            var veterinarian = await _context.Veterinarians
                .FirstOrDefaultAsync(m => m.VetId == id);

            if (veterinarian == null) return NotFound();
            return View(veterinarian);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("VetId,VetLastName,VetFirstName,VetSpecialty,VetPhone,VetEmail")] Veterinarian veterinarian)
        {
            // PG-MIGRATION (7j): case-insensitive duplicate-email check.
            if (!string.IsNullOrWhiteSpace(veterinarian.VetEmail) &&
                _context.Veterinarians.Any(v =>
                    v.VetEmail != null &&
                    v.VetEmail.ToLower() == veterinarian.VetEmail.ToLower()))
            {
                ModelState.AddModelError("VetEmail",
                    "The provided email is already in use. Please choose a different email.");
                return View(veterinarian);
            }

            if (ModelState.IsValid)
            {
                _context.Add(veterinarian);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(veterinarian);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Veterinarians == null) return NotFound();

            var veterinarian = await _context.Veterinarians.FindAsync(id);
            if (veterinarian == null) return NotFound();
            return View(veterinarian);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("VetId,VetLastName,VetFirstName,VetSpecialty,VetPhone,VetEmail")] Veterinarian veterinarian)
        {
            if (id != veterinarian.VetId) return NotFound();

            // PG-MIGRATION (7j): case-insensitive duplicate-email check (excluding self).
            if (!string.IsNullOrWhiteSpace(veterinarian.VetEmail) &&
                _context.Veterinarians.Any(v =>
                    v.VetId != veterinarian.VetId &&
                    v.VetEmail != null &&
                    v.VetEmail.ToLower() == veterinarian.VetEmail.ToLower()))
            {
                ModelState.AddModelError("VetEmail",
                    "The provided email is already in use. Please choose a different email.");
                return View(veterinarian);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(veterinarian);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VeterinarianExists(veterinarian.VetId)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(veterinarian);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Veterinarians == null) return NotFound();

            var veterinarian = await _context.Veterinarians
                .FirstOrDefaultAsync(m => m.VetId == id);

            if (veterinarian == null) return NotFound();
            return View(veterinarian);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Veterinarians == null)
                return Problem("Entity set 'AnimalCareContext.Veterinarians' is null.");

            var veterinarian = await _context.Veterinarians.FindAsync(id);
            if (veterinarian != null) _context.Veterinarians.Remove(veterinarian);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VeterinarianExists(int id)
        {
            return (_context.Veterinarians?.Any(e => e.VetId == id)).GetValueOrDefault();
        }
    }
}