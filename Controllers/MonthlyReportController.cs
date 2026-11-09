using AnimalCareVeterinaryClinicDBFirst.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnimalCareVeterinaryClinicDBFirst.Controllers
{
    /// <summary>
    /// Controller responsible for managing monthly report generation and display
    /// Handles filtering, viewing, and exporting monthly statistics for administrators
    /// </summary>
    [Authorize(Roles = "Administrator")]
    public class MonthlyReportController : Controller
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
        /// - Controller is ready to handle monthly report requests
        /// </post-condition>
        public MonthlyReportController(AnimalCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays the initial monthly report page without any data
        /// </summary>
        /// <returns>View with empty report list awaiting filter criteria</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - User must have Administrator role
        /// - Database context must be initialized
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Index view with empty report list
        /// - ViewBag.FilterApplied is set to false to prevent chart rendering
        /// - User can apply filters to generate report data
        /// </post-condition>
        public async Task<IActionResult> Index()
        {
            // Set filter flag to false on initial page load
            ViewBag.FilterApplied = false;
            // Initialize empty report list so chart won't render on first load
            var report = new List<MonthlyReport>(); // empty so chart won't render on first load
            return View(report);
        }

        /// <summary>
        /// Processes filter criteria and displays filtered monthly report data
        /// </summary>
        /// <param name="month">Optional month filter (1-12)</param>
        /// <param name="year">Optional year filter</param>
        /// <returns>View displaying filtered monthly report data with statistics</returns>
        /// <pre-condition>
        /// - User must be authenticated as Administrator
        /// - Form must include valid anti-forgery token
        /// - Month value must be between 1-12 if provided
        /// - Year value must be valid if provided
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Index view with filtered report data
        /// - ViewBag.FilterApplied is true if any filter was applied
        /// - ViewBag.Month and ViewBag.Year contain the applied filter values
        /// - Report list contains records matching the filter criteria
        /// - If no filters applied, returns all monthly report records
        /// </post-condition>
        [HttpPost]
        public async Task<IActionResult> Index(int? month, int? year)
        {
            // Start with all monthly reports
            var query = _context.MonthlyReports.AsQueryable();
            bool filterApplied = false;

            // Apply month filter if provided
            if (month.HasValue)
            {
                query = query.Where(r => r.Month == month);
                filterApplied = true;
            }

            // Apply year filter if provided
            if (year.HasValue)
            {
                query = query.Where(r => r.Year == year);
                filterApplied = true;
            }

            // Store filter values in ViewBag for display
            ViewBag.Month = month;
            ViewBag.Year = year;
            ViewBag.FilterApplied = filterApplied;

            // Execute query and retrieve filtered report data
            var report = await query.ToListAsync();
            return View(report);
        }

        /// <summary>
        /// Exports all monthly report data to an Excel-compatible file
        /// </summary>
        /// <returns>Excel file containing all monthly report records</returns>
        /// <pre-condition>
        /// - User must be authenticated as Administrator
        /// - MonthlyReports table must be accessible
        /// </pre-condition>
        /// <post-condition>
        /// - Returns tab-delimited text file with .xls extension
        /// - File contains headers: Month, Year, Appointments, Canceled, Vets, Occupancy
        /// - File includes all records from MonthlyReports table
        /// - File is UTF-8 encoded with Excel MIME type
        /// - Downloaded file is named "MonthlyReport.xls"
        /// </post-condition>
        public IActionResult ExportExcel()
        {
            // Retrieve all monthly report data from database
            var data = _context.MonthlyReports.ToList();

            // Build tab-delimited CSV content with header row
            var csv = "Month\tYear\tAppointments\tCanceled\tVets\tOccupancy\n";

            // Append each report record as a tab-delimited row
            foreach (var r in data)
                csv += $"{r.Month}\t{r.Year}\t{r.NumberOfAppointments}\t{r.NumberCanceled}\t{r.NumberOfActiveVets}\t{r.OccupancyRate}\n";

            // Return file as Excel download
            return File(System.Text.Encoding.UTF8.GetBytes(csv),
                "application/vnd.ms-excel", "MonthlyReport.xls");
        }

    }
}