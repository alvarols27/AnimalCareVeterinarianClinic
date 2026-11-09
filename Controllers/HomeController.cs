using AnimalCareVeterinaryClinicDBFirst.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AnimalCareVeterinaryClinicDBFirst.Controllers
{
    /// <summary>
    /// Controller responsible for managing the home page and general application views
    /// Handles dashboard display with role-based appointment counts and error pages
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AnimalCareContext _context;
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Constructor that initializes the controller with database context and logger
        /// </summary>
        /// <param name="context">Entity Framework database context for data operations</param>
        /// <param name="logger">Logger instance for recording application events and errors</param>
        /// <pre-condition>
        /// - Database context must be properly configured in dependency injection
        /// - Logger must be properly configured in dependency injection
        /// </pre-condition>
        /// <post-condition>
        /// - Controller is ready to handle home page and error requests
        /// </post-condition>
        public HomeController(AnimalCareContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Displays the home page dashboard with today's appointment count based on user role
        /// </summary>
        /// <returns>View displaying the home page with appointment statistics</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Database context must be initialized
        /// - User account must exist in database with valid RoleId
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Index view with ViewBag.TodayAppointments populated
        /// - Admin/Receptionist: Count includes all non-canceled appointments for today
        /// - Veterinarian: Count includes only their own non-canceled appointments for today
        /// - If user not found or role not determined, appointment count defaults to 0
        /// </post-condition>
        public async Task<IActionResult> Index()
        {
            // Calculate today's date range for appointment filtering
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            int todayAppointments = 0;

            // Check authenticated user
            // Extract user ID from authentication claims
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (User.Identity?.IsAuthenticated == true && int.TryParse(userIdString, out int userId))
            {
                // Retrieve user account information from database
                var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user != null)
                {
                    // Load the user's role to determine access level
                    var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == user.RoleId);
                    if (role != null)
                    {
                        // ADMIN + RECEPTIONIST → count all appointments
                        // Administrative roles can see all appointments in the system
                        if (role.RoleName == "Administrator" || role.RoleName == "Receptionist")
                        {
                            todayAppointments = await _context.Appointments
                                .CountAsync(a =>
                                    a.StartDateTime >= today &&
                                    a.StartDateTime < tomorrow &&
                                    a.AppointmentStatus != "Canceled");
                        }
                        // VET → count only their appointments
                        // Veterinarians only see their own scheduled appointments
                        else if (role.RoleName == "Veterinarian")
                        {
                            // Find the veterinarian record by matching user's first and last name
                            var vet = await _context.Veterinarians
                                .FirstOrDefaultAsync(v =>
                                    v.VetFirstName.Trim().ToLower() == user.UserFirstName.Trim().ToLower() &&
                                    v.VetLastName.Trim().ToLower() == user.UserLastName.Trim().ToLower());
                            if (vet != null)
                            {
                                // Count only appointments assigned to this specific veterinarian
                                todayAppointments = await _context.Appointments
                                    .CountAsync(a =>
                                        a.VetId == vet.VetId &&
                                        a.StartDateTime >= today &&
                                        a.StartDateTime < tomorrow &&
                                        a.AppointmentStatus != "Canceled");
                            }
                        }
                    }
                }
            }

            // Pass appointment count to view for display on dashboard
            ViewBag.TodayAppointments = todayAppointments;
            return View();
        }

        /// <summary>
        /// Displays the privacy policy page
        /// </summary>
        /// <returns>View showing the application's privacy policy</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Privacy view
        /// </post-condition>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays the error page when an unhandled exception occurs
        /// </summary>
        /// <returns>View displaying error details with request identifier</returns>
        /// <pre-condition>
        /// - Can be called at any time when an error occurs
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Error view with ErrorViewModel containing request ID
        /// - Response is not cached to ensure fresh error information
        /// - RequestId is either the current Activity ID or HttpContext TraceIdentifier
        /// </post-condition>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}