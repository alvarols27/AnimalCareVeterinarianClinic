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
    /// Controller responsible for managing notification records in the veterinary clinic system
    /// Handles CRUD operations for notifications and enforces role-based access control
    /// </summary>
    [Authorize(Roles = "Administrator, Veterinarian, Receptionist")]
    public class NotificationsController : Controller
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
        /// - Controller is ready to handle notification requests
        /// </post-condition>
        public NotificationsController(AnimalCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays notifications based on the logged-in user's role
        /// Administrators and Receptionists see all notifications, Veterinarians see only their own
        /// </summary>
        /// <returns>View displaying notifications filtered by role permissions</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - User must have Administrator, Veterinarian, or Receptionist role
        /// - User's account must exist in the database with a valid RoleId
        /// - Database context must be initialized
        /// </pre-condition>
        /// <post-condition>
        /// - Admin/Receptionist: Returns Index view with all notifications
        /// - Veterinarian: Returns Index view with only notifications for their appointments
        /// - Returns Unauthorized if user ID cannot be parsed or user doesn't exist
        /// - Returns Unauthorized if user's role cannot be found
        /// - Returns Forbid if user has an unrecognized role
        /// - Returns empty list for veterinarians not found in Veterinarians table
        /// </post-condition>
        public async Task<IActionResult> Index()
        {
            // Safely get the logged-in user ID from claims
            // Extract user ID from authentication claims
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            // Load the logged-in user
            // Retrieve the user account from database
            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return Unauthorized();

            // Load the user's role
            // Retrieve the role information associated with the user
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == user.RoleId);
            if (role == null)
                return Unauthorized();

            // Admins & Receptionists → see all notifications
            // Grant full access to all notification records for administrative roles
            if (role.RoleName == "Administrator" || role.RoleName == "Receptionist")
            {
                var allNotifications = await _context.Notifications
                    .Include(n => n.Appointment)
                        .ThenInclude(a => a.Vet)
                    .ToListAsync();

                return View(allNotifications);
            }

            // Veterinarians → see their own notifications ONLY
            // Filter notifications to show only those assigned to the logged-in veterinarian
            if (role.RoleName == "Veterinarian")
            {
                // First get the vet based on first/last name (since no UserId FK exists)
                // Match veterinarian record by comparing user's first and last name
                var vet = await _context.Veterinarians
                    .FirstOrDefaultAsync(v =>
                        v.VetFirstName.Trim().ToLower() == user.UserFirstName.Trim().ToLower() &&
                        v.VetLastName.Trim().ToLower() == user.UserLastName.Trim().ToLower());

                if (vet == null)
                {
                    return View(new List<Notification>());
                }

                // Now load ONLY notifications where the appointment is assigned to this vet
                // Filter notifications by veterinarian ID from associated appointments
                var vetNotifications = await _context.Notifications
                    .Where(n => n.Appointment.VetId == vet.VetId)
                    .Include(n => n.Appointment)
                        .ThenInclude(a => a.Vet)
                    .ToListAsync();

                return View(vetNotifications);
            }

            // Any other case → forbidden
            // Return Forbid if role is not recognized
            return Forbid();
        }




        /// <summary>
        /// Displays detailed information about a specific notification
        /// </summary>
        /// <param name="id">The unique identifier of the notification to display</param>
        /// <returns>View showing notification details with appointment, animal, and veterinarian information</returns>
        /// <pre-condition>
        /// - User must be authenticated with valid role
        /// - Notification ID must be provided
        /// - Database must be accessible
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Details view if notification is found
        /// - Returns NotFound result if ID is null or notification doesn't exist
        /// - Includes Appointment with Animal and Vet information via eager loading
        /// </post-condition>
        /// GET: Notifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Validate that ID was provided
            if (id == null || _context.Notifications == null)
            {
                return NotFound();
            }

            // Retrieve notification with related appointment, animal, and veterinarian data
            var notification = await _context.Notifications
                .Include(n => n.Appointment)
                    .ThenInclude(a => a.Animal)
                .Include(n => n.Appointment)
                    .ThenInclude(a => a.Vet)
                .FirstOrDefaultAsync(m => m.NotificationId == id);

            // Check if notification was found
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        /// <summary>
        /// Displays the form for creating a new notification record
        /// </summary>
        /// <returns>View with empty notification form and list of available appointments</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - User must have appropriate role permissions
        /// - At least one appointment must exist in the database
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Create view with empty form
        /// - ViewData contains appointment list with animal name, vet name, and date for dropdown selection
        /// </post-condition>
        /// GET: Notifications/Create
        public IActionResult Create()
        {
            // Populate appointment dropdown with detailed information
            ViewData["AppointmentId"] = new SelectList(
                _context.Appointments
                    .Include(a => a.Animal)
                    .Include(a => a.Vet)
                    .Select(a => new
                    {
                        AppointmentId = a.AppointmentId,
                        DisplayName = a.Animal.AnimalName + " - " +
                                    a.Vet.VetFirstName + " " + a.Vet.VetLastName +
                                    " - " + a.StartDateTime.ToString("yyyy-MM-dd HH:mm")
                    }),
                "AppointmentId",
                "DisplayName"
            );
            return View();
        }

        /// <summary>
        /// Processes the form submission to create a new notification record
        /// </summary>
        /// <param name="notification">Notification object containing form data from the create view</param>
        /// <returns>Redirects to Index on success, returns Create view with errors on failure</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Notification object must contain valid data
        /// - AppointmentId must reference an existing appointment
        /// - Form must include valid anti-forgery token
        /// </pre-condition>
        /// <post-condition>
        /// - New notification record is saved to database on success
        /// - Redirects to Index view after successful creation
        /// - Returns Create view with validation errors if data is invalid
        /// - Returns Create view with error message if database save fails
        /// </post-condition>
        /// POST: Notifications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppointmentId,NotificationType,SendDate,Channel,NotificationStatus")] Notification notification)
        {
            // Remove navigation property from model state validation
            // This is not posted from the form and would cause validation errors
            ModelState.Remove("Appointment");

            if (ModelState.IsValid)
            {
                try
                {
                    // Add new notification to database context
                    _context.Add(notification);
                    // Save changes to database
                    await _context.SaveChangesAsync();
                    // Redirect to Index page on success
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Add error message to model state if save fails
                    ModelState.AddModelError("", "Unable to save notification. Error: " + (ex.InnerException?.Message ?? ex.Message));
                }
            }

            // Reload dropdown if validation fails
            // Repopulate appointment dropdown with current selection
            ViewData["AppointmentId"] = new SelectList(
                _context.Appointments
                    .Include(a => a.Animal)
                    .Include(a => a.Vet)
                    .Select(a => new
                    {
                        AppointmentId = a.AppointmentId,
                        DisplayName = a.Animal.AnimalName + " - " +
                                    a.Vet.VetFirstName + " " + a.Vet.VetLastName +
                                    " - " + a.StartDateTime.ToString("yyyy-MM-dd HH:mm")
                    }),
                "AppointmentId",
                "DisplayName",
                notification.AppointmentId
            );

            // Return view with validation errors
            return View(notification);
        }

        /// <summary>
        /// Displays the form for editing an existing notification record
        /// </summary>
        /// <param name="id">The unique identifier of the notification to edit</param>
        /// <returns>View with populated notification form data</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Notification ID must be provided and valid
        /// - Notification must exist in database
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Edit view with notification data if found
        /// - Returns NotFound if ID is null or notification doesn't exist
        /// - ViewData contains appointment list for dropdown
        /// </post-condition>
        /// GET: Notifications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Validate ID parameter
            if (id == null || _context.Notifications == null)
            {
                return NotFound();
            }

            // Find the notification by ID
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            // Populate appointment dropdown with current selection
            ViewData["AppointmentId"] = new SelectList(
                _context.Appointments
                    .Include(a => a.Animal)
                    .Include(a => a.Vet)
                    .Select(a => new
                    {
                        AppointmentId = a.AppointmentId,
                        DisplayName = a.Animal.AnimalName + " - " +
                                    a.Vet.VetFirstName + " " + a.Vet.VetLastName +
                                    " - " + a.StartDateTime.ToString("yyyy-MM-dd HH:mm")
                    }),
                "AppointmentId",
                "DisplayName",
                notification.AppointmentId
            );

            return View(notification);
        }

        /// <summary>
        /// Processes the form submission to update an existing notification record
        /// </summary>
        /// <param name="id">The unique identifier of the notification to update</param>
        /// <param name="notification">Notification object containing updated form data</param>
        /// <returns>Redirects to Index on success, returns Edit view on failure</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Notification ID must match the ID in the notification object
        /// - Notification must exist in database
        /// - Form must include valid anti-forgery token
        /// </pre-condition>
        /// <post-condition>
        /// - Notification record is updated in database on success
        /// - Redirects to Index view after successful update
        /// - Returns NotFound if IDs don't match or notification doesn't exist
        /// - Returns Edit view with validation errors if data is invalid
        /// - Handles concurrency conflicts appropriately
        /// </post-condition>
        /// POST: Notifications/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NotificationId,AppointmentId,NotificationType,SendDate,Channel,NotificationStatus")] Notification notification)
        {
            // Verify that route ID matches notification ID
            if (id != notification.NotificationId)
            {
                return NotFound();
            }

            // Remove navigation property from validation
            ModelState.Remove("Appointment");

            if (ModelState.IsValid)
            {
                try
                {
                    // Update notification in database context
                    _context.Update(notification);
                    // Save changes to database
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Check if notification still exists
                    if (!NotificationExists(notification.NotificationId))
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
            // Repopulate appointment dropdown with current selection
            ViewData["AppointmentId"] = new SelectList(
                _context.Appointments
                    .Include(a => a.Animal)
                    .Include(a => a.Vet)
                    .Select(a => new
                    {
                        AppointmentId = a.AppointmentId,
                        DisplayName = a.Animal.AnimalName + " - " +
                                    a.Vet.VetFirstName + " " + a.Vet.VetLastName +
                                    " - " + a.StartDateTime.ToString("yyyy-MM-dd HH:mm")
                    }),
                "AppointmentId",
                "DisplayName",
                notification.AppointmentId
            );

            // Return view with validation errors
            return View(notification);
        }

        /// <summary>
        /// Displays confirmation page before deleting a notification record
        /// </summary>
        /// <param name="id">The unique identifier of the notification to delete</param>
        /// <returns>View showing notification details for deletion confirmation</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Notification ID must be provided
        /// - Notification must exist in database
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Delete view with notification data if found
        /// - Returns NotFound if ID is null or notification doesn't exist
        /// - Includes Appointment with Animal and Vet information via eager loading
        /// </post-condition>
        /// GET: Notifications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Validate ID parameter
            if (id == null || _context.Notifications == null)
            {
                return NotFound();
            }

            // Retrieve notification with appointment, animal, and veterinarian information
            var notification = await _context.Notifications
                .Include(n => n.Appointment)
                    .ThenInclude(a => a.Animal)
                .Include(n => n.Appointment)
                    .ThenInclude(a => a.Vet)
                .FirstOrDefaultAsync(m => m.NotificationId == id);

            // Check if notification exists
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        /// <summary>
        /// Processes the confirmed deletion of a notification record from the database
        /// </summary>
        /// <param name="id">The unique identifier of the notification to delete</param>
        /// <returns>Redirects to Index after successful deletion</returns>
        /// <pre-condition>
        /// - User must be authenticated with appropriate permissions
        /// - Notification ID must be valid
        /// - Form must include valid anti-forgery token
        /// - User must have confirmed deletion intent
        /// </pre-condition>
        /// <post-condition>
        /// - Notification record is permanently removed from database
        /// - Redirects to Index view after deletion
        /// - Returns Problem result if Notifications DbSet is null
        /// </post-condition>
        /// POST: Notifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if Notifications table is accessible
            if (_context.Notifications == null)
            {
                return Problem("Entity set 'AnimalCareContext.Notifications' is null.");
            }

            // Find the notification by ID
            var notification = await _context.Notifications.FindAsync(id);
            // Remove notification if found
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
            }

            // Save changes to database
            await _context.SaveChangesAsync();
            // Redirect to Index page
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Helper method to check if a notification exists in the database
        /// </summary>
        /// <param name="id">The unique identifier of the notification to check</param>
        /// <returns>True if notification exists, false otherwise</returns>
        /// <pre-condition>
        /// - Database context must be initialized
        /// - Notification ID must be provided
        /// </pre-condition>
        /// <post-condition>
        /// - Returns true if notification with given ID exists
        /// - Returns false if notification doesn't exist or Notifications DbSet is null
        /// </post-condition>
        private bool NotificationExists(int id)
        {
            return (_context.Notifications?.Any(e => e.NotificationId == id)).GetValueOrDefault();
        }
    }
}