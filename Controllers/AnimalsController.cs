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
    /// Controller responsible for managing animal records in the veterinary clinic system
    /// Handles CRUD operations for animals and enforces role-based access control
    /// </summary>
    [Authorize(Roles = "Administrator,Veterinarian,Receptionist")]
    public class AnimalsController : Controller
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
        /// - Controller is ready to handle animal-related requests
        /// </post-condition>
        public AnimalsController(AnimalCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all animals with their associated owners from the database
        /// </summary>
        /// <returns>View displaying all animals in a table format</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - User must have Administrator, Veterinarian, or Receptionist role
        /// - Database context must be initialized
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Index view with list of Animal objects
        /// - Each animal includes its Owner navigation property loaded via eager loading
        /// - If no animals exist, returns empty list
        /// </post-condition>
        // GET: Animals
        public async Task<IActionResult> Index()
        {
            // Load all animals with their associated owners using eager loading
            var animalCareContext = _context.Animals.Include(a => a.Owner);
            // Return the view with the list of animals
            return View(await animalCareContext.ToListAsync());
        }

        /// <summary>
        /// Displays detailed information about a specific animal including medical history
        /// </summary>
        /// <param name="id">The unique identifier of the animal to display</param>
        /// <returns>View showing animal details, owner info, and medical history</returns>
        /// <pre-condition>
        /// - User must be authenticated with valid role
        /// - Animal ID must be provided
        /// - Database must be accessible
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Details view if animal is found
        /// - Returns NotFound result if animal ID is null or animal doesn't exist
        /// - Includes Owner and MedicalHistories with Vet information via eager loading
        /// </post-condition>
        // GET: Animals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Validate that ID was provided
            if (id == null || _context.Animals == null)
            {
                return NotFound();
            }

            // Retrieve animal with related owner, medical histories, and veterinarian data
            var animal = await _context.Animals
                .Include(a => a.Owner)
                .Include(a => a.MedicalHistories)
                .ThenInclude(m => m.Vet)
                .FirstOrDefaultAsync(m => m.AnimalId == id);

            // Check if animal was found
            if (animal == null)
            {
                return NotFound();
            }

            return View(animal);
        }

        /// <summary>
        /// Displays the form for creating a new animal record
        /// </summary>
        /// <returns>View with empty animal form and list of available owners</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - User must have appropriate role permissions
        /// - At least one owner must exist in the database
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Create view with empty form
        /// - ViewData contains owner list for dropdown selection
        /// </post-condition>
        // GET: Animals/Create
        public IActionResult Create()
        {
            // Populate owner dropdown with concatenated full names
            ViewData["OwnerId"] = new SelectList(
                _context.Owners.Select(o => new
                {
                    OwnerId = o.OwnerId,
                    FullName = o.OwnerFirstName + " " + o.OwnerLastName
                }),
                "OwnerId",
                "FullName"
            );
            return View();
        }

        /// <summary>
        /// Processes the form submission to create a new animal record
        /// </summary>
        /// <param name="animal">Animal object containing form data from the create view</param>
        /// <returns>Redirects to Index on success, returns Create view with errors on failure</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Animal object must contain valid data
        /// - OwnerId must reference an existing owner
        /// - Form must include valid anti-forgery token
        /// </pre-condition>
        /// <post-condition>
        /// - New animal record is saved to database on success
        /// - Redirects to Index view after successful creation
        /// - Returns Create view with validation errors if data is invalid
        /// - Returns Create view with error message if database save fails
        /// </post-condition>
        // POST: Animals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AnimalName,Species,Breed,Sex,BirthDate,AnimalWeight,OwnerId")] Animal animal)
        {

            // Remove navigation properties from model state validation
            // These are not posted from the form and would cause validation errors
            ModelState.Remove("Owner");
            ModelState.Remove("Appointments");
            ModelState.Remove("MedicalHistories");

            if (ModelState.IsValid)
            {
                try
                {
                    // Add new animal to database context
                    _context.Add(animal);

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

            // Reload owner list if validation fails to repopulate dropdown
            ViewData["OwnerId"] = new SelectList(
                _context.Owners.Select(o => new
                {
                    OwnerId = o.OwnerId,
                    FullName = o.OwnerFirstName + " " + o.OwnerLastName
                }),
                "OwnerId",
                "FullName",
                animal.OwnerId
            );

            // Return view with validation errors
            return View(animal);
        }

        /// <summary>
        /// Displays the form for editing an existing animal record
        /// </summary>
        /// <param name="id">The unique identifier of the animal to edit</param>
        /// <returns>View with populated animal form data</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Animal ID must be provided and valid
        /// - Animal must exist in database
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Edit view with animal data if found
        /// - Returns NotFound if ID is null or animal doesn't exist
        /// - ViewData contains owner list for dropdown
        /// </post-condition>
        // GET: Animals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Validate ID parameter
            if (id == null || _context.Animals == null)
            {
                return NotFound();
            }

            // Find the animal by ID
            var animal = await _context.Animals.FindAsync(id);
            if (animal == null)
            {
                return NotFound();
            }

            // Populate owner dropdown with current selection
            ViewData["OwnerId"] = new SelectList(
                _context.Owners.Select(o => new
                {
                    OwnerId = o.OwnerId,
                    FullName = o.OwnerFirstName + " " + o.OwnerLastName
                }),
                "OwnerId",
                "FullName",
                animal.OwnerId
            );
            return View(animal);
        }

        /// <summary>
        /// Processes the form submission to update an existing animal record
        /// Implements role-based field editing restrictions
        /// </summary>
        /// <param name="id">The unique identifier of the animal to update</param>
        /// <param name="animal">Animal object containing updated form data</param>
        /// <returns>Redirects to Index on success, returns Edit view on failure</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Animal ID must match the ID in the animal object
        /// - Animal must exist in database
        /// - Form must include valid anti-forgery token
        /// </pre-condition>
        /// <post-condition>
        /// - Animal record is updated in database
        /// - Admin/Receptionist: All fields can be modified
        /// - Veterinarian: Only AnimalWeight field can be modified
        /// - Redirects to Index view after successful update
        /// - Returns NotFound if IDs don't match or animal doesn't exist
        /// </post-condition>
        // POST: Animals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Animal animal)
        {
            // Verify that route ID matches animal ID
            if (id != animal.AnimalId)
                return NotFound();

            // Remove navigation properties from validation
            ModelState.Remove("Owner");
            ModelState.Remove("Appointments");
            ModelState.Remove("MedicalHistories");

            // Retrieve existing animal from database
            var existing = await _context.Animals.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Apply role-based editing restrictions
            if (User.IsInRole("Administrator") || User.IsInRole("Receptionist"))
            {
                // Admin & Receptionist can edit all fields
                existing.AnimalName = animal.AnimalName;
                existing.Species = animal.Species;
                existing.Breed = animal.Breed;
                existing.Sex = animal.Sex;
                existing.BirthDate = animal.BirthDate;
                existing.OwnerId = animal.OwnerId;
                existing.AnimalWeight = animal.AnimalWeight;
            }
            else
            {
                // Vets can ONLY edit weight
                existing.AnimalWeight = animal.AnimalWeight;
            }

            // Save changes to database
            await _context.SaveChangesAsync();

            // Redirect to Index page
            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// Displays confirmation page before deleting an animal record
        /// </summary>
        /// <param name="id">The unique identifier of the animal to delete</param>
        /// <returns>View showing animal details for deletion confirmation</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Animal ID must be provided
        /// - Animal must exist in database
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Delete view with animal data if found
        /// - Returns NotFound if ID is null or animal doesn't exist
        /// - Includes Owner information via eager loading
        /// </post-condition>
        // GET: Animals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Validate ID parameter
            if (id == null || _context.Animals == null)
            {
                return NotFound();
            }

            // Retrieve animal with owner information
            var animal = await _context.Animals
                .Include(a => a.Owner)
                .FirstOrDefaultAsync(m => m.AnimalId == id);

            // Check if animal exists
            if (animal == null)
            {
                return NotFound();
            }

            return View(animal);
        }

        /// <summary>
        /// Processes the confirmed deletion of an animal record from the database
        /// </summary>
        /// <param name="id">The unique identifier of the animal to delete</param>
        /// <returns>Redirects to Index after successful deletion</returns>
        /// <pre-condition>
        /// - User must be authenticated with appropriate permissions
        /// - Animal ID must be valid
        /// - Form must include valid anti-forgery token
        /// - User must have confirmed deletion intent
        /// </pre-condition>
        /// <post-condition>
        /// - Animal record is permanently removed from database
        /// - Related records are handled by cascade delete (appointments, medical history)
        /// - Redirects to Index view after deletion
        /// - Returns Problem result if Animals DbSet is null
        /// </post-condition>
        // POST: Animals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if Animals table is accessible
            if (_context.Animals == null)
            {
                return Problem("Entity set 'AnimalCareContext.Animals'  is null.");
            }

            // Find the animal by ID
            var animal = await _context.Animals.FindAsync(id);

            // Remove animal if found (cascade delete handles related records)
            if (animal != null)
            {
                _context.Animals.Remove(animal);
            }

            // Save changes to database
            await _context.SaveChangesAsync();

            // Redirect to Index page
            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// Helper method to check if an animal exists in the database
        /// </summary>
        /// <param name="id">The unique identifier of the animal to check</param>
        /// <returns>True if animal exists, false otherwise</returns>
        /// <pre-condition>
        /// - Database context must be initialized
        /// - Animal ID must be provided
        /// </pre-condition>
        /// <post-condition>
        /// - Returns true if animal with given ID exists
        /// - Returns false if animal doesn't exist or Animals DbSet is null
        /// </post-condition>
        private bool AnimalExists(int id)
        {
            return (_context.Animals?.Any(e => e.AnimalId == id)).GetValueOrDefault();
        }
    }
}