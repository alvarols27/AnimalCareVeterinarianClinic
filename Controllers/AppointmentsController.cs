using AnimalCareVeterinaryClinicDBFirst.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimalCareVeterinaryClinicDBFirst.Controllers
{
    [Authorize(Roles = "Administrator,Veterinarian,Receptionist")]
    public class AppointmentsController : Controller
    {
        private readonly AnimalCareContext _context;

        public AppointmentsController(AnimalCareContext context)
        {
            _context = context;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return Unauthorized();

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == user.RoleId);
            if (role == null) return Unauthorized();

            IQueryable<Appointment> query;

            if (role.RoleName == "Administrator" || role.RoleName == "Receptionist")
            {
                query = _context.Appointments
                    .Include(a => a.Animal)
                    .Include(a => a.Vet);
            }
            else if (role.RoleName == "Veterinarian")
            {
                // PG-MIGRATION (7a): case-sensitive PG comparison — normalise both sides.
                var firstLower = (user.UserFirstName ?? "").Trim().ToLower();
                var lastLower  = (user.UserLastName  ?? "").Trim().ToLower();

                var vet = await _context.Veterinarians
                    .FirstOrDefaultAsync(v =>
                        v.VetFirstName.Trim().ToLower() == firstLower &&
                        v.VetLastName.Trim().ToLower()  == lastLower);

                if (vet == null)
                    return View(new AppointmentIndexViewModel());

                query = _context.Appointments
                    .Where(a => a.VetId == vet.VetId)
                    .Include(a => a.Animal)
                    .Include(a => a.Vet);
            }
            else
            {
                return Forbid();
            }

            var allAppointments = await query.ToListAsync();
            var today = DateTime.Today;

            var vm = new AppointmentIndexViewModel
            {
                Today = allAppointments
                    .Where(a => a.StartDateTime.Date == today && a.AppointmentStatus != "Canceled")
                    .OrderBy(a => a.StartDateTime)
                    .ToList(),

                Upcoming = allAppointments
                    .Where(a => a.StartDateTime.Date > today && a.AppointmentStatus == "Planned")
                    .OrderBy(a => a.StartDateTime)
                    .ToList(),

                Past = allAppointments
                    .Where(a => a.StartDateTime.Date < today ||
                                a.AppointmentStatus == "Completed" ||
                                a.AppointmentStatus == "Canceled")
                    .OrderByDescending(a => a.StartDateTime)
                    .ToList()
            };

            return View(vm);
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Appointments == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Animal)
                .Include(a => a.Vet)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (appointment == null) return NotFound();
            return View(appointment);
        }

        // GET: Appointments/Create
        [Authorize(Roles = "Administrator,Receptionist")]
        public IActionResult Create()
        {
            var model = new Appointment
            {
                StartDateTime = DateTime.Today,
                EndDateTime   = DateTime.Today.AddMinutes(30)
            };

            ViewData["AnimalId"] = new SelectList(
                _context.Animals.Select(a => new
                {
                    a.AnimalId,
                    DisplayName = a.AnimalName + " (" + a.Species + ")"
                }),
                "AnimalId", "DisplayName");

            ViewData["VetId"] = new SelectList(
                _context.Veterinarians.Select(v => new
                {
                    v.VetId,
                    DisplayName = v.VetFirstName + " " + v.VetLastName + " - " + v.VetSpecialty
                }),
                "VetId", "DisplayName");

            return View(model);
        }

        // ===============================
        // GET: Available Time Slots (AJAX)
        // ===============================
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimes(int vetId, DateTime date, int? excludeId)
        {
            var schedule = await _context.Schedules
                .FirstOrDefaultAsync(s => s.VetId == vetId && s.ScheduleDate == date);

            if (schedule == null)
                return Json(new List<string>());

            var appointments = await _context.Appointments
                .Where(a => a.VetId == vetId &&
                            a.StartDateTime.Date == date.Date &&
                            a.AppointmentId != excludeId &&
                            a.AppointmentStatus != "Canceled")
                .ToListAsync();

            var slots = GenerateTimeSlots(schedule.StartTime, schedule.EndTime, 30);

            foreach (var appt in appointments)
            {
                slots = slots
                    .Where(slot =>
                        !(slot >= appt.StartDateTime.TimeOfDay &&
                          slot <  appt.EndDateTime.TimeOfDay))
                    .ToList();
            }

            return Json(slots.Select(t => t.ToString(@"hh\:mm")).ToList());
        }

        // ===============================
        // GET: Vet Working Dates (for calendar)
        // ===============================
        [HttpGet]
        public async Task<IActionResult> GetVetScheduledDates(int vetId)
        {
            var dates = await _context.Schedules
                .Where(s => s.VetId == vetId)
                .Select(s => s.ScheduleDate)
                .ToListAsync();

            if (dates == null)
                return Json(new { hasSchedule = false, dates = new List<string>() });

            var formatted = dates.Select(d => d.ToString("yyyy-MM-dd")).ToList();
            return Json(formatted);
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("VetId,AnimalId,StartDateTime,EndDateTime,Purpose,AppointmentStatus")] Appointment appointment,
            [FromServices] NotificationService notify)
        {
            Console.WriteLine("START RAW: " + appointment.StartDateTime.ToString("o"));
            Console.WriteLine("END RAW:   " + appointment.EndDateTime.ToString("o"));

            // PG-MIGRATION (7b): PostgreSQL column type is 'timestamp without time zone'
            // (Npgsql maps System.DateTime to it). SpecifyKind(Local) preserves the
            // original SQL Server-style "naive local time" semantics. No change in behaviour;
            // kept for clarity. Remove if you migrate to DateTimeOffset later.
            appointment.StartDateTime = DateTime.SpecifyKind(appointment.StartDateTime, DateTimeKind.Local);
            appointment.EndDateTime   = DateTime.SpecifyKind(appointment.EndDateTime,   DateTimeKind.Local);

            ModelState.Remove("Animal");
            ModelState.Remove("Vet");
            ModelState.Remove("Notifications");

            var now = DateTime.Now;
            var oneYear = now.AddYears(1);

            if (appointment.StartDateTime < now)
                ModelState.AddModelError("StartDateTime", "Start time cannot be in the past.");

            if (appointment.EndDateTime < appointment.StartDateTime)
                ModelState.AddModelError("EndDateTime", "End time must be after the start time.");

            if (appointment.StartDateTime > oneYear)
                ModelState.AddModelError("StartDateTime", "Start time cannot be more than 1 year ahead.");

            if (appointment.EndDateTime > oneYear)
                ModelState.AddModelError("EndDateTime", "End time cannot be more than 1 year ahead.");

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(appointment);
                return View(appointment);
            }

            try
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();

                // PG-MIGRATION (7h): Option A — C# creates the notification, DB trigger
                // 'trg_AddReminderNotification' is removed from the SQL script.
                // If you chose Option B instead, DELETE this notify.Create(...) call.
                await notify.Create(
                    appointment.AppointmentId,
                    "Reminder",
                    "Sent",
                    "Email");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unable to save appointment. Error: "
                    + (ex.InnerException?.Message ?? ex.Message));
                PopulateDropdowns(appointment);
                return View(appointment);
            }
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Appointments == null)
                return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            ViewData["AnimalId"] = new SelectList(
                _context.Animals.Select(a => new
                {
                    a.AnimalId,
                    DisplayName = a.AnimalName + " (" + a.Species + ")"
                }),
                "AnimalId", "DisplayName", appointment.AnimalId);

            ViewData["VetId"] = new SelectList(
                _context.Veterinarians.Select(v => new
                {
                    v.VetId,
                    DisplayName = v.VetFirstName + " " + v.VetLastName + " - " + v.VetSpecialty
                }),
                "VetId", "DisplayName", appointment.VetId);

            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [Authorize(Roles = "Administrator,Receptionist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("AppointmentId,VetId,AnimalId,StartDateTime,EndDateTime,Purpose,AppointmentStatus")] Appointment appointment,
            [FromServices] NotificationService notify)
        {
            if (id != appointment.AppointmentId) return NotFound();

            ModelState.Remove("Animal");
            ModelState.Remove("Vet");
            ModelState.Remove("Notifications");

            if (ModelState.IsValid)
            {
                try
                {
                    var oldAppointment = await _context.Appointments
                        .AsNoTracking()
                        .FirstOrDefaultAsync(a => a.AppointmentId == id);

                    _context.Update(appointment);
                    await _context.SaveChangesAsync();

                    bool changed =
                        oldAppointment.VetId != appointment.VetId ||
                        oldAppointment.AnimalId != appointment.AnimalId ||
                        oldAppointment.StartDateTime != appointment.StartDateTime ||
                        oldAppointment.EndDateTime != appointment.EndDateTime ||
                        oldAppointment.Purpose != appointment.Purpose;

                    if (changed)
                        await notify.Create(appointment.AppointmentId, "Updated", "Pending", "Email");

                    if (appointment.AppointmentStatus == "Canceled")
                        await notify.Create(appointment.AppointmentId, "Cancellation", "Pending", "Email");
                    else if (appointment.AppointmentStatus == "Completed")
                        await notify.Create(appointment.AppointmentId, "Completed", "Pending", "Email");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.AppointmentId)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["AnimalId"] = new SelectList(
                _context.Animals.Select(a => new
                {
                    a.AnimalId,
                    DisplayName = a.AnimalName + " (" + a.Species + ")"
                }),
                "AnimalId", "DisplayName", appointment.AnimalId);

            ViewData["VetId"] = new SelectList(
                _context.Veterinarians.Select(v => new
                {
                    v.VetId,
                    DisplayName = v.VetFirstName + " " + v.VetLastName + " - " + v.VetSpecialty
                }),
                "VetId", "DisplayName", appointment.VetId);

            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Appointments == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Animal)
                .Include(a => a.Vet)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (appointment == null) return NotFound();
            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrator,Receptionist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Appointments == null)
                return Problem("Entity set 'AnimalCareContext.Appointments' is null.");

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null) _context.Appointments.Remove(appointment);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Appointments/UpdateMedical/5
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> UpdateMedical(int id)
        {
            var appt = await _context.Appointments
                .Include(a => a.Animal)
                .Include(a => a.Vet)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (appt == null) return NotFound();

            var vm = new UpdateMedicalNotes
            {
                AppointmentId = appt.AppointmentId,
                AppointmentStatus = appt.AppointmentStatus,
                AnimalName = appt.Animal.AnimalName,
                Species = appt.Animal.Species,
                Observations = "",
                Diagnosis = "",
                Treatment = ""
            };

            return View(vm);
        }

        // POST: Appointments/UpdateMedical
        [Authorize(Roles = "Veterinarian")]
        [HttpPost]
        public async Task<IActionResult> UpdateMedical(UpdateMedicalNotes vm)
        {
            var appt = await _context.Appointments
                .Include(a => a.Animal)
                .Include(a => a.Vet)
                .FirstOrDefaultAsync(a => a.AppointmentId == vm.AppointmentId);

            if (appt == null) return NotFound();

            appt.AppointmentStatus = vm.AppointmentStatus;

            if (vm.AppointmentStatus == "Completed")
            {
                var med = new MedicalHistory
                {
                    AnimalId = appt.AnimalId,
                    VetId = appt.VetId,
                    ConsultationDate = appt.EndDateTime.Date,
                    Observations = vm.Observations,
                    Diagnosis = vm.Diagnosis,
                    Treatment = vm.Treatment
                };

                _context.MedicalHistories.Add(med);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return (_context.Appointments?.Any(e => e.AppointmentId == id)).GetValueOrDefault();
        }

        private static List<TimeSpan> GenerateTimeSlots(TimeSpan start, TimeSpan end, int minutes)
        {
            var slots = new List<TimeSpan>();
            var cursor = start;

            while (cursor < end)
            {
                slots.Add(cursor);
                cursor = cursor.Add(TimeSpan.FromMinutes(minutes));
            }

            return slots;
        }

        private void PopulateDropdowns(Appointment appointment)
        {
            ViewData["AnimalId"] = new SelectList(
                _context.Animals.Select(a => new
                {
                    AnimalId = a.AnimalId,
                    DisplayName = a.AnimalName + " (" + a.Species + ")"
                }),
                "AnimalId", "DisplayName", appointment.AnimalId);

            ViewData["VetId"] = new SelectList(
                _context.Veterinarians.Select(v => new
                {
                    VetId = v.VetId,
                    DisplayName = v.VetFirstName + " " + v.VetLastName + " - " + v.VetSpecialty
                }),
                "VetId", "DisplayName", appointment.VetId);
        }
    }
}