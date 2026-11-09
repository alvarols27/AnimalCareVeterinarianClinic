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
    /// <summary>
    /// Controller responsible for managing veterinarian schedule records in the veterinary clinic system
    /// Handles CRUD operations for schedules, enforces role-based access control, and validates business hours
    /// </summary>
    [Authorize(Roles = "Administrator,Veterinarian,Receptionist")]
    public class SchedulesController : Controller
    {
        private readonly AnimalCareContext _context;

        /// <summary>
        /// Constructor that initializes the controller with database context
        /// </summary>
        /// <param name="context">Entity Framework database context for data operations</param>
        /// <pre-condition>
        /// - Database context must be properly configured in dependency injection
        /// </pre-condition>
        /// <post-condition>
        /// - Controller is ready to handle schedule-related requests
        /// </post-condition>
        public SchedulesController(AnimalCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays schedules based on the logged-in user's role with real-time busy status
        /// Administrators and Receptionists see all schedules, Veterinarians see only their own
        /// </summary>
        /// <returns>View displaying schedules filtered by role permissions with busy status indicators</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - User must have Administrator, Veterinarian, or Receptionist role
        /// - User's account must exist in the database with a valid RoleId
        /// - Database context must be initialized
        /// </pre-condition>
        /// <post-condition>
        /// - Admin/Receptionist: Returns Index view with all schedules
        /// - Veterinarian: Returns Index view with only their own schedules
        /// - Each schedule includes IsVetBusyNow flag indicating current appointment status
        /// - Returns Unauthorized if user ID cannot be parsed or user doesn't exist
        /// - Returns Unauthorized if user's role cannot be found
        /// - Returns empty list for veterinarians not found in Veterinarians table
        /// </post-condition>
        // GET: Schedules
        public async Task<IActionResult> Index()
        {
            // 1. Logged-in user
            // Extract user ID from authentication claims
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            // Retrieve the user account from database
            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return Unauthorized();

            // Load the user's role to determine access level
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == user.RoleId);
            if (role == null) return Unauthorized();

            // 2. Base query (includes vet info)
            // Start with all schedules including veterinarian information
            IQueryable<Schedule> query = _context.Schedules.Include(s => s.Vet);

            // 3. Veterinarian: filter to only their own schedules
            // Filter schedules to show only those assigned to the logged-in veterinarian
            if (role.RoleName == "Veterinarian")
            {
                // Match veterinarian record by comparing user's first and last name
                var vet = await _context.Veterinarians
                    .FirstOrDefaultAsync(v =>
                        v.VetFirstName == user.UserFirstName &&
                        v.VetLastName == user.UserLastName);

                if (vet == null)
                    return View(new List<Schedule>());

                // Apply veterinarian filter to query
                query = query.Where(s => s.VetId == vet.VetId);
            }

            // 4. Load schedules
            // Execute query and retrieve schedule data
            var schedules = await query.ToListAsync();

            // 5. Determine "Busy Now" for each vet
            // Calculate current busy status based on active appointments
            var now = DateTime.Now;

            foreach (var s in schedules)
            {
                // Check if veterinarian has an active non-canceled appointment right now
                bool isBusy = await _context.Appointments.AnyAsync(a =>
                    a.VetId == s.VetId &&
                    a.StartDateTime <= now &&
                    a.EndDateTime >= now &&
                    a.AppointmentStatus != "Canceled"
                );

                // Attach runtime "busy" status on the fly
                // Set real-time busy indicator for display in view
                s.IsVetBusyNow = isBusy;
            }

            return View(schedules);
        }


        /// <summary>
        /// Displays detailed information about a specific schedule
        /// </summary>
        /// <param name="id">The unique identifier of the schedule to display</param>
        /// <returns>View showing schedule details with veterinarian information</returns>
        /// <pre-condition>
        /// - User must be authenticated with valid role
        /// - Schedule ID must be provided
        /// - Database must be accessible
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Details view if schedule is found
        /// - Returns NotFound result if ID is null or schedule doesn't exist
        /// - Includes Vet information via eager loading
        /// </post-condition>
        // GET: Schedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Validate that ID was provided
            if (id == null || _context.Schedules == null)
            {
                return NotFound();
            }

            // Retrieve schedule with related veterinarian data
            var schedule = await _context.Schedules
                .Include(s => s.Vet)
                .FirstOrDefaultAsync(m => m.ScheduleId == id);
            // Check if schedule was found
            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }


        /// <summary>
        /// Displays the form for creating a new schedule record with role-based defaults
        /// </summary>
        /// <returns>View with schedule form and veterinarian dropdown list</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - User must have appropriate role permissions
        /// - At least one veterinarian must exist in the database
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Create view with default schedule values (8:00-18:00, today's date)
        /// - ViewData contains veterinarian list with names and specialties for dropdown
        /// - If logged-in user is a veterinarian, their VetId is pre-selected
        /// - ViewBag.IsVetLoggedIn indicates if current user is a veterinarian
        /// </post-condition>
        // GET: Schedules/Create
        public async Task<IActionResult> Create()
        {
            // Build vet dropdown
            // Prepare veterinarian dropdown with full name and specialty
            var vets = await _context.Veterinarians
                .Select(v => new
                {
                    v.VetId,
                    DisplayName = v.VetFirstName + " " + v.VetLastName + " - " + v.VetSpecialty
                })
                .ToListAsync();

            int? vetIdForLoggedInVet = null;
            bool isVetLoggedIn = false;

            // Identify logged-in user
            // Extract user ID from authentication claims
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdString, out int userId))
            {
                // Retrieve user account information
                var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user != null)
                {
                    // Load user's role to check if they are a veterinarian
                    var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == user.RoleId);

                    if (role?.RoleName == "Veterinarian")
                    {
                        isVetLoggedIn = true;

                        // Find veterinarian record for logged-in vet user
                        var vet = await _context.Veterinarians
                            .FirstOrDefaultAsync(v =>
                                v.VetFirstName == user.UserFirstName &&
                                v.VetLastName == user.UserLastName);

                        // Store vet ID for pre-selection in dropdown
                        vetIdForLoggedInVet = vet?.VetId;
                    }
                }
            }

            // Pass veterinarian status to view for UI logic
            ViewBag.IsVetLoggedIn = isVetLoggedIn;

            // Populate dropdown
            // Create veterinarian dropdown with optional pre-selected value
            ViewData["VetId"] = new SelectList(vets, "VetId", "DisplayName", vetIdForLoggedInVet);

            // Default schedule values (weekday hours)
            // Initialize new schedule with default weekday business hours
            var model = new Schedule
            {
                ScheduleDate = DateTime.Today,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(18, 0, 0),
                ScheduleStatus = "Free",
                VetId = vetIdForLoggedInVet ?? 0
            };

            return View(model);
        }


        /// <summary>
        /// Processes the form submission to create a new schedule record with business hours validation
        /// </summary>
        /// <param name="schedule">Schedule object containing form data from the create view</param>
        /// <returns>Redirects to Index on success, returns Create view with errors on failure</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Schedule object must contain valid data
        /// - VetId must reference an existing veterinarian
        /// - Form must include valid anti-forgery token
        /// </pre-condition>
        /// <post-condition>
        /// - New schedule record is saved to database on success
        /// - Validates against clinic business hours (Weekdays: 8:00-18:00, Saturday: 9:00-16:00, Sunday: Closed)
        /// - Validates start time is before end time
        /// - Validates schedule is not in the past
        /// - Redirects to Index view after successful creation
        /// - Returns Create view with validation errors if data is invalid
        /// </post-condition>
        // POST: Schedules/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VetId,ScheduleDate,StartTime,EndTime,ScheduleStatus")] Schedule schedule)
        {
            // Remove navigation property from validation
            ModelState.Remove("Vet");

            // Determine day of week for business hours validation
            var day = schedule.ScheduleDate.DayOfWeek;

            // Business hours by day
            // Define opening time based on day of week
            TimeSpan open = day switch
            {
                DayOfWeek.Saturday => new TimeSpan(9, 0, 0),
                DayOfWeek.Sunday => TimeSpan.Zero, // Closed, handled below
                _ => new TimeSpan(8, 0, 0) // Weekday
            };

            // Define closing time based on day of week
            TimeSpan close = day switch
            {
                DayOfWeek.Saturday => new TimeSpan(16, 0, 0),
                DayOfWeek.Sunday => TimeSpan.Zero,
                _ => new TimeSpan(18, 0, 0)
            };

            /* ================================
               FULL VALIDATION SECTION
            ================================= */

            // Closed Sundays
            // Validate that clinic is not closed on the selected date
            if (day == DayOfWeek.Sunday)
                ModelState.AddModelError("ScheduleDate", "Clinic is closed on Sundays.");

            // Required time fields
            // Ensure start time is provided
            if (schedule.StartTime == default)
                ModelState.AddModelError("StartTime", "Start time is required.");

            // Ensure end time is provided
            if (schedule.EndTime == default)
                ModelState.AddModelError("EndTime", "End time is required.");

            // Only check if provided
            // Validate time ranges only if both times are provided
            if (schedule.StartTime != default && schedule.EndTime != default)
            {
                // Validate start time is within business hours
                if (schedule.StartTime < open || schedule.StartTime >= close)
                    ModelState.AddModelError("StartTime",
                        $"Start must be between {open:hh\\:mm} and {close:hh\\:mm}");

                // Validate end time is after start time
                if (schedule.EndTime <= schedule.StartTime)
                    ModelState.AddModelError("EndTime", "End time must be after start time.");

                // Validate end time does not exceed closing time
                if (schedule.EndTime > close)
                    ModelState.AddModelError("EndTime",
                        $"End time must be no later than {close:hh\\:mm}.");
            }

            // Prevent past schedules
            // Validate schedule date is not in the past
            if (schedule.ScheduleDate < DateTime.Today)
                ModelState.AddModelError("ScheduleDate", "Cannot schedule in the past.");

            /* ================================
               Return if invalid
            ================================= */
            // Return view with errors if validation failed
            if (!ModelState.IsValid)
            {
                // Reload veterinarian dropdown with current selection
                ViewData["VetId"] = new SelectList(
                    _context.Veterinarians.Select(v => new
                    {
                        v.VetId,
                        DisplayName = v.VetFirstName + " " + v.VetLastName + " - " + v.VetSpecialty
                    }),
                    "VetId", "DisplayName", schedule.VetId
                );

                return View(schedule);
            }

            /* ================================
               Save
            ================================= */
            // Add new schedule to database context
            _context.Add(schedule);
            // Save changes to database
            await _context.SaveChangesAsync();

            // Redirect to Index page on success
            return RedirectToAction(nameof(Index));
        }



        /// <summary>
        /// Displays the form for editing an existing schedule record
        /// </summary>
        /// <param name="id">The unique identifier of the schedule to edit</param>
        /// <returns>View with populated schedule form data</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Schedule ID must be provided and valid
        /// - Schedule must exist in database
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Edit view with schedule data if found
        /// - Returns NotFound if ID is null or schedule doesn't exist
        /// - ViewData contains veterinarian list for dropdown
        /// </post-condition>
        // GET: Schedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Validate ID parameter
            if (id == null || _context.Schedules == null)
            {
                return NotFound();
            }

            // Find the schedule by ID
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            // Show vet names instead of IDs
            // Populate veterinarian dropdown with current selection
            ViewData["VetId"] = new SelectList(
                _context.Veterinarians.Select(v => new
                {
                    VetId = v.VetId,
                    DisplayName = v.VetFirstName + " " + v.VetLastName + " - " + v.VetSpecialty
                }),
                "VetId",
                "DisplayName",
                schedule.VetId
            );
            return View(schedule);
        }

        /// <summary>
        /// Processes the form submission to update an existing schedule record
        /// </summary>
        /// <param name="id">The unique identifier of the schedule to update</param>
        /// <param name="schedule">Schedule object containing updated form data</param>
        /// <returns>Redirects to Index on success, returns Edit view on failure</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Schedule ID must match the ID in the schedule object
        /// - Schedule must exist in database
        /// - Form must include valid anti-forgery token
        /// </pre-condition>
        /// <post-condition>
        /// - Schedule record is updated in database on success
        /// - Redirects to Index view after successful update
        /// - Returns NotFound if IDs don't match or schedule doesn't exist
        /// - Returns Edit view with validation errors if data is invalid
        /// - Handles concurrency conflicts appropriately
        /// </post-condition>
        // POST: Schedules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ScheduleId,VetId,ScheduleDate,StartTime,EndTime,ScheduleStatus")] Schedule schedule)
        {
            // Verify that route ID matches schedule ID
            if (id != schedule.ScheduleId)
            {
                return NotFound();
            }

            // Remove navigation property from validation
            ModelState.Remove("Vet");

            if (ModelState.IsValid)
            {
                try
                {
                    // Update schedule in database context
                    _context.Update(schedule);
                    // Save changes to database
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Check if schedule still exists
                    if (!ScheduleExists(schedule.ScheduleId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        // Rethrow exception if it's a different concurrency issue
                        throw;
                    }
                }
                // Redirect to Index page on success
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdown if validation fails
            // Repopulate veterinarian dropdown with current selection
            ViewData["VetId"] = new SelectList(
                _context.Veterinarians.Select(v => new
                {
                    VetId = v.VetId,
                    DisplayName = v.VetFirstName + " " + v.VetLastName + " - " + v.VetSpecialty
                }),
                "VetId",
                "DisplayName",
                schedule.VetId
            );
            // Return view with validation errors
            return View(schedule);
        }

        /// <summary>
        /// Displays confirmation page before deleting a schedule record
        /// </summary>
        /// <param name="id">The unique identifier of the schedule to delete</param>
        /// <returns>View showing schedule details for deletion confirmation</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Schedule ID must be provided
        /// - Schedule must exist in database
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Delete view with schedule data if found
        /// - Returns NotFound if ID is null or schedule doesn't exist
        /// - Includes Vet information via eager loading
        /// </post-condition>
        // GET: Schedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Validate ID parameter
            if (id == null || _context.Schedules == null)
            {
                return NotFound();
            }

            // Retrieve schedule with veterinarian information
            var schedule = await _context.Schedules
                .Include(s => s.Vet)
                .FirstOrDefaultAsync(m => m.ScheduleId == id);
            // Check if schedule exists
            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        /// <summary>
        /// Processes the confirmed deletion of a schedule record from the database
        /// </summary>
        /// <param name="id">The unique identifier of the schedule to delete</param>
        /// <returns>Redirects to Index after successful deletion</returns>
        /// <pre-condition>
        /// - User must be authenticated with appropriate permissions
        /// - Schedule ID must be valid
        /// - Form must include valid anti-forgery token
        /// - User must have confirmed deletion intent
        /// </pre-condition>
        /// <post-condition>
        /// - Schedule record is permanently removed from database
        /// - Redirects to Index view after deletion
        /// - Returns Problem result if Schedules DbSet is null
        /// </post-condition>
        // POST: Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if Schedules table is accessible
            if (_context.Schedules == null)
            {
                return Problem("Entity set 'AnimalCareContext.Schedules' is null.");
            }
            // Find the schedule by ID
            var schedule = await _context.Schedules.FindAsync(id);
            // Remove schedule if found
            if (schedule != null)
            {
                _context.Schedules.Remove(schedule);
            }

            // Save changes to database
            await _context.SaveChangesAsync();
            // Redirect to Index page
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Helper method to check if a schedule exists in the database
        /// </summary>
        /// <param name="id">The unique identifier of the schedule to check</param>
        /// <returns>True if schedule exists, false otherwise</returns>
        /// <pre-condition>
        /// - Database context must be initialized
        /// - Schedule ID must be provided
        /// </pre-condition>
        /// <post-condition>
        /// - Returns true if schedule with given ID exists
        /// - Returns false if schedule doesn't exist or Schedules DbSet is null
        /// </post-condition>
        private bool ScheduleExists(int id)
        {
            return (_context.Schedules?.Any(e => e.ScheduleId == id)).GetValueOrDefault();
        }
    }
}