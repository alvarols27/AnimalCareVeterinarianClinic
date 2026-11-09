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
    /// Controller responsible for managing medical history records in the veterinary clinic system
    /// Handles CRUD operations for medical histories and enforces role-based access control
    /// </summary>
    [Authorize(Roles = "Administrator,Veterinarian")]
    public class MedicalHistoriesController : Controller
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
        /// - Controller is ready to handle medical history requests
        /// </post-condition>
        public MedicalHistoriesController(AnimalCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all medical histories with their associated animals and veterinarians
        /// </summary>
        /// <returns>View displaying all medical histories in a table format</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - User must have Administrator or Veterinarian role
        /// - Database context must be initialized
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Index view with list of MedicalHistory objects
        /// - Each medical history includes Animal and Vet navigation properties via eager loading
        /// - If no medical histories exist, returns empty list
        /// </post-condition>
        // GET: MedicalHistories
        public async Task<IActionResult> Index()
        {
            // Load all medical histories with their associated animals and veterinarians
            var animalCareContext = _context.MedicalHistories
                .Include(m => m.Animal)
                .Include(m => m.Vet);
            // Return the view with the list of medical histories
            return View(await animalCareContext.ToListAsync());
        }

        /// <summary>
        /// Displays detailed information about a specific medical history record
        /// </summary>
        /// <param name="id">The unique identifier of the medical history to display</param>
        /// <returns>View showing medical history details with animal and veterinarian information</returns>
        /// <pre-condition>
        /// - User must be authenticated with valid role
        /// - Medical history ID must be provided
        /// - Database must be accessible
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Details view if medical history is found
        /// - Returns NotFound result if ID is null or medical history doesn't exist
        /// - Includes Animal and Vet information via eager loading
        /// </post-condition>
        // GET: MedicalHistories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Validate that ID was provided
            if (id == null || _context.MedicalHistories == null)
            {
                return NotFound();
            }

            // Retrieve medical history with related animal and veterinarian data
            var medicalHistory = await _context.MedicalHistories
                .Include(m => m.Animal)
                .Include(m => m.Vet)
                .FirstOrDefaultAsync(m => m.HistoryId == id);
            // Check if medical history was found
            if (medicalHistory == null)
            {
                return NotFound();
            }

            return View(medicalHistory);
        }

        /// <summary>
        /// Displays the form for creating a new medical history record
        /// </summary>
        /// <returns>View with empty medical history form and dropdown lists for animals and veterinarians</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - User must have appropriate role permissions
        /// - At least one animal and one veterinarian must exist in the database
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Create view with empty form
        /// - ViewData contains animal list with names and species for dropdown selection
        /// - ViewData contains veterinarian list with names and specialties for dropdown selection
        /// </post-condition>
        // GET: MedicalHistories/Create
        public IActionResult Create()
        {
            // Show animal names instead of IDs
            // Populate animal dropdown with animal name and species
            ViewData["AnimalId"] = new SelectList(
                _context.Animals.Select(a => new
                {
                    AnimalId = a.AnimalId,
                    DisplayName = a.AnimalName + " (" + a.Species + ")"
                }),
                "AnimalId",
                "DisplayName"
            );

            // Show vet names instead of IDs
            // Populate veterinarian dropdown with full name and specialty
            ViewData["VetId"] = new SelectList(
                _context.Veterinarians.Select(v => new
                {
                    VetId = v.VetId,
                    DisplayName = v.VetFirstName + " " + v.VetLastName + " - " + v.VetSpecialty
                }),
                "VetId",
                "DisplayName"
            );

            return View();
        }

        /// <summary>
        /// Processes the form submission to create a new medical history record
        /// </summary>
        /// <param name="medicalHistory">MedicalHistory object containing form data from the create view</param>
        /// <returns>Redirects to Index on success, returns Create view with errors on failure</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - MedicalHistory object must contain valid data
        /// - AnimalId must reference an existing animal
        /// - VetId must reference an existing veterinarian
        /// - Form must include valid anti-forgery token
        /// </pre-condition>
        /// <post-condition>
        /// - New medical history record is saved to database on success
        /// - Redirects to Index view after successful creation
        /// - Returns Create view with validation errors if data is invalid
        /// - Returns Create view with error message if database save fails
        /// </post-condition>
        // POST: MedicalHistories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AnimalId,ConsultationDate,Observations,Diagnosis,Treatment,VetId")] MedicalHistory medicalHistory)
        {
            // Remove navigation properties from model state validation
            // These are not posted from the form and would cause validation errors
            ModelState.Remove("Animal");
            ModelState.Remove("Vet");

            if (ModelState.IsValid)
            {
                try
                {
                    // Add new medical history to database context
                    _context.Add(medicalHistory);
                    // Save changes to database
                    await _context.SaveChangesAsync();
                    // Redirect to Index page on success
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Add error message to model state if save fails
                    ModelState.AddModelError("", "Unable to save changes. Error: " + (ex.InnerException?.Message ?? ex.Message));
                }
            }

            // Reload dropdowns if validation fails
            // Repopulate animal dropdown with current selection
            ViewData["AnimalId"] = new SelectList(
                _context.Animals.Select(a => new
                {
                    AnimalId = a.AnimalId,
                    DisplayName = a.AnimalName + " (" + a.Species + ")"
                }),
                "AnimalId",
                "DisplayName",
                medicalHistory.AnimalId
            );

            // Repopulate veterinarian dropdown with current selection
            ViewData["VetId"] = new SelectList(
                _context.Veterinarians.Select(v => new
                {
                    VetId = v.VetId,
                    DisplayName = v.VetFirstName + " " + v.VetLastName + " - " + v.VetSpecialty
                }),
                "VetId",
                "DisplayName",
                medicalHistory.VetId
            );

            // Return view with validation errors
            return View(medicalHistory);
        }

        /// <summary>
        /// Displays the form for editing an existing medical history record
        /// </summary>
        /// <param name="id">The unique identifier of the medical history to edit</param>
        /// <returns>View with populated medical history form data</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Medical history ID must be provided and valid
        /// - Medical history must exist in database
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Edit view with medical history data if found
        /// - Returns NotFound if ID is null or medical history doesn't exist
        /// - ViewData contains animal and veterinarian lists for dropdowns
        /// </post-condition>
        // GET: MedicalHistories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Validate ID parameter
            if (id == null || _context.MedicalHistories == null)
            {
                return NotFound();
            }

            // Find the medical history by ID
            var medicalHistory = await _context.MedicalHistories.FindAsync(id);
            if (medicalHistory == null)
            {
                return NotFound();
            }

            // Show animal names instead of IDs
            // Populate animal dropdown with current selection
            ViewData["AnimalId"] = new SelectList(
                _context.Animals.Select(a => new
                {
                    AnimalId = a.AnimalId,
                    DisplayName = a.AnimalName + " (" + a.Species + ")"
                }),
                "AnimalId",
                "DisplayName",
                medicalHistory.AnimalId
            );

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
                medicalHistory.VetId
            );

            return View(medicalHistory);
        }

        /// <summary>
        /// Processes the form submission to update an existing medical history record
        /// </summary>
        /// <param name="id">The unique identifier of the medical history to update</param>
        /// <param name="medicalHistory">MedicalHistory object containing updated form data</param>
        /// <returns>Redirects to Index on success, returns Edit view on failure</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Medical history ID must match the ID in the medicalHistory object
        /// - Medical history must exist in database
        /// - Form must include valid anti-forgery token
        /// </pre-condition>
        /// <post-condition>
        /// - Medical history record is updated in database on success
        /// - Redirects to Index view after successful update
        /// - Returns NotFound if IDs don't match or medical history doesn't exist
        /// - Returns Edit view with validation errors if data is invalid
        /// - Handles concurrency conflicts appropriately
        /// </post-condition>
        // POST: MedicalHistories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HistoryId,AnimalId,ConsultationDate,Observations,Diagnosis,Treatment,VetId")] MedicalHistory medicalHistory)
        {
            // Verify that route ID matches medical history ID
            if (id != medicalHistory.HistoryId)
            {
                return NotFound();
            }

            // Remove navigation properties from validation
            ModelState.Remove("Animal");
            ModelState.Remove("Vet");

            if (ModelState.IsValid)
            {
                try
                {
                    // Update medical history in database context
                    _context.Update(medicalHistory);
                    // Save changes to database
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Check if medical history still exists
                    if (!MedicalHistoryExists(medicalHistory.HistoryId))
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

            // Repopulate animal dropdown if validation fails
            ViewData["AnimalId"] = new SelectList(
                _context.Animals.Select(a => new
                {
                    AnimalId = a.AnimalId,
                    DisplayName = a.AnimalName + " (" + a.Species + ")"
                }),
                "AnimalId",
                "DisplayName",
                medicalHistory.AnimalId
            );

            // Repopulate veterinarian dropdown if validation fails
            ViewData["VetId"] = new SelectList(
                _context.Veterinarians.Select(v => new
                {
                    VetId = v.VetId,
                    DisplayName = v.VetFirstName + " " + v.VetLastName + " - " + v.VetSpecialty
                }),
                "VetId",
                "DisplayName",
                medicalHistory.VetId
            );

            // Return view with validation errors
            return View(medicalHistory);
        }

        /// <summary>
        /// Displays confirmation page before deleting a medical history record
        /// </summary>
        /// <param name="id">The unique identifier of the medical history to delete</param>
        /// <returns>View showing medical history details for deletion confirmation</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Medical history ID must be provided
        /// - Medical history must exist in database
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Delete view with medical history data if found
        /// - Returns NotFound if ID is null or medical history doesn't exist
        /// - Includes Animal and Vet information via eager loading
        /// </post-condition>
        // GET: MedicalHistories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Validate ID parameter
            if (id == null || _context.MedicalHistories == null)
            {
                return NotFound();
            }

            // Retrieve medical history with animal and veterinarian information
            var medicalHistory = await _context.MedicalHistories
                .Include(m => m.Animal)
                .Include(m => m.Vet)
                .FirstOrDefaultAsync(m => m.HistoryId == id);
            // Check if medical history exists
            if (medicalHistory == null)
            {
                return NotFound();
            }

            return View(medicalHistory);
        }

        /// <summary>
        /// Processes the confirmed deletion of a medical history record from the database
        /// </summary>
        /// <param name="id">The unique identifier of the medical history to delete</param>
        /// <returns>Redirects to Index after successful deletion</returns>
        /// <pre-condition>
        /// - User must be authenticated with appropriate permissions
        /// - Medical history ID must be valid
        /// - Form must include valid anti-forgery token
        /// - User must have confirmed deletion intent
        /// </pre-condition>
        /// <post-condition>
        /// - Medical history record is permanently removed from database
        /// - Redirects to Index view after deletion
        /// - Returns Problem result if MedicalHistories DbSet is null
        /// </post-condition>
        // POST: MedicalHistories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if MedicalHistories table is accessible
            if (_context.MedicalHistories == null)
            {
                return Problem("Entity set 'AnimalCareContext.MedicalHistories' is null.");
            }
            // Find the medical history by ID
            var medicalHistory = await _context.MedicalHistories.FindAsync(id);
            // Remove medical history if found
            if (medicalHistory != null)
            {
                _context.MedicalHistories.Remove(medicalHistory);
            }

            // Save changes to database
            await _context.SaveChangesAsync();
            // Redirect to Index page
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Helper method to check if a medical history record exists in the database
        /// </summary>
        /// <param name="id">The unique identifier of the medical history to check</param>
        /// <returns>True if medical history exists, false otherwise</returns>
        /// <pre-condition>
        /// - Database context must be initialized
        /// - Medical history ID must be provided
        /// </pre-condition>
        /// <post-condition>
        /// - Returns true if medical history with given ID exists
        /// - Returns false if medical history doesn't exist or MedicalHistories DbSet is null
        /// </post-condition>
        private bool MedicalHistoryExists(int id)
        {
            return (_context.MedicalHistories?.Any(e => e.HistoryId == id)).GetValueOrDefault();
        }
    }
}