using AnimalCareVeterinaryClinicDBFirst.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AnimalCareVeterinaryClinicDBFirst.Controllers
{
    /// <summary>
    /// Controller responsible for managing daily agenda views in the veterinary clinic system
    /// Handles role-based access to appointment schedules with different views for different roles
    /// </summary>
    [Authorize(Roles = "Administrator,Receptionist, Veterinarian")]
    public class DailyAgendaController : Controller
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
        /// - Controller is ready to handle daily agenda requests
        /// </post-condition>
        public DailyAgendaController(AnimalCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays the daily agenda based on the logged-in user's role
        /// Administrators and Receptionists see all agendas, Veterinarians see only their own
        /// </summary>
        /// <returns>View displaying agenda items filtered by role permissions</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - User must have Administrator, Receptionist, or Veterinarian role
        /// - User's account must exist in the database with a valid RoleId
        /// - Database context must be initialized
        /// </pre-condition>
        /// <post-condition>
        /// - Admin/Receptionist: Returns Index view with all daily agenda items
        /// - Veterinarian: Returns Index view with only their own agenda items for today
        /// - Returns Unauthorized if user ID cannot be parsed or user doesn't exist
        /// - Returns Unauthorized if user's role cannot be found
        /// - Returns Forbid if user has an unrecognized role
        /// </post-condition>
        public async Task<IActionResult> Index()
        {
            // Who is logged in?
            // Extract user ID from claims and validate it's a valid integer
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            // Load the user
            // Retrieve the user account from database using the parsed user ID
            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return Unauthorized();

            // Load role
            // Retrieve the role information associated with the user
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == user.RoleId);
            if (role == null) return Unauthorized();

            // Admin + Receptionist → See all agendas
            // Grant full access to all daily agenda items for administrative roles
            if (role.RoleName == "Administrator" || role.RoleName == "Receptionist")
            {
                var allAgenda = await _context.DailyAgenda.ToListAsync();
                return View(allAgenda);
            }

            // Veterinarian --> Only their own agenda
            // Filter agenda to show only items assigned to the logged-in veterinarian for today
            if (role.RoleName == "Veterinarian")
            {
                // "Adam Dale" --> we need "Dale Adam"
                // Convert user name from "FirstName LastName" format to "LastName FirstName" format
                // to match the veterinarian name storage format in the DailyAgenda table
                var vetFullName = $"{user.UserLastName} {user.UserFirstName}".Trim().ToLower();
                var vetAgenda = await _context.DailyAgenda
                    .Where(a => a.VeterinarianName != null &&
                                a.VeterinarianName.Trim().ToLower() == vetFullName && a.Date == DateTime.Today)
                    .ToListAsync();
                return View(vetAgenda);
            }

            // Return Forbid if role is not recognized or authorized
            return Forbid();
        }
    }
}