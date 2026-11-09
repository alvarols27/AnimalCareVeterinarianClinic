using AnimalCareVeterinaryClinicDBFirst.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AnimalCareVeterinaryClinicDBFirst.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UserAccountsController : Controller
    {
        private readonly AnimalCareContext _context;

        public UserAccountsController(AnimalCareContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = _context.UserAccounts.Include(u => u.Role);
            return View(await users.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.UserAccounts
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null) return NotFound();
            return View(user);
        }

        public IActionResult Create()
        {
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreation model)
        {
            // PG-MIGRATION (7j): case-insensitive duplicate-login check.
            if (!string.IsNullOrWhiteSpace(model.UserLogin) &&
                _context.UserAccounts.Any(u =>
                    u.UserLogin.ToLower() == model.UserLogin.Trim().ToLower()))
            {
                ModelState.AddModelError("UserLogin",
                    "The provided login is already in use. Please choose a different login.");
                ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", model.RoleId);
                return View(model);
            }

            ModelState.Remove("Role");

            if (ModelState.IsValid)
            {
                var userAccount = new UserAccount
                {
                    UserLogin     = model.UserLogin?.Trim(),
                    UserPassword  = model.UserPassword,
                    UserFirstName = model.UserFirstName,
                    UserLastName  = model.UserLastName,
                    RoleId        = model.RoleId,
                    UserActive    = model.UserActive
                };

                _context.Add(userAccount);
                await _context.SaveChangesAsync();

                // PG-MIGRATION (7j): case-insensitive duplicate-email check for vet profile.
                if (!string.IsNullOrWhiteSpace(model.VetEmail) &&
                    _context.Veterinarians.Any(v =>
                        v.VetEmail != null &&
                        v.VetEmail.ToLower() == model.VetEmail.ToLower()))
                {
                    ModelState.AddModelError("VetEmail",
                        "The provided email is already in use. Please choose a different email.");
                    ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", model.RoleId);
                    return View(model);
                }

                if (model.RoleId == 2) // Veterinarian
                {
                    var vet = new Veterinarian
                    {
                        VetFirstName = model.UserFirstName,
                        VetLastName  = model.UserLastName,
                        VetSpecialty = model.VetSpecialty,
                        VetPhone     = model.VetPhone,
                        VetEmail     = model.VetEmail
                    };

                    _context.Veterinarians.Add(vet);
                    await _context.SaveChangesAsync();

                    var schedule = new Schedule
                    {
                        VetId = vet.VetId,
                        ScheduleDate = DateTime.Today,
                        StartTime = TimeSpan.Parse("09:00"),
                        EndTime = TimeSpan.Parse("17:00"),
                        ScheduleStatus = "Free"
                    };

                    _context.Schedules.Add(schedule);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", model.RoleId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.UserAccounts.FindAsync(id);
            if (user == null) return NotFound();

            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("UserId,UserLogin,UserPassword,UserFirstName,UserLastName,RoleId,UserActive")] UserAccount userAccount)
        {
            if (id != userAccount.UserId) return NotFound();

            // PG-MIGRATION (7j): case-insensitive duplicate-login check (excluding self).
            if (!string.IsNullOrWhiteSpace(userAccount.UserLogin) &&
                _context.UserAccounts.Any(u =>
                    u.UserId != userAccount.UserId &&
                    u.UserLogin.ToLower() == userAccount.UserLogin.Trim().ToLower()))
            {
                ModelState.AddModelError("UserLogin",
                    "The provided login is already in use. Please choose a different login.");
                ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", userAccount.RoleId);
                return View(userAccount);
            }

            ModelState.Remove("Role");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userAccount);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.UserAccounts.Any(e => e.UserId == userAccount.UserId))
                        return NotFound();
                    throw;
                }
            }

            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", userAccount.RoleId);
            return View(userAccount);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.UserAccounts
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.UserAccounts.FindAsync(id);
            if (user != null) _context.UserAccounts.Remove(user);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}