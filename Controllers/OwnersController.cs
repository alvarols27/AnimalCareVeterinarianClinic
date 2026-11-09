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
    /// Controller responsible for managing pet owner records in the veterinary clinic system
    /// Handles CRUD operations for owners and enforces role-based access control
    /// </summary>
    [Authorize(Roles = "Administrator,Veterinarian,Receptionist")]
    public class OwnersController : Controller
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
        /// - Controller is ready to handle owner-related requests
        /// </post-condition>
        public OwnersController(AnimalCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all pet owners from the database
        /// </summary>
        /// <returns>View displaying all owners in a table format</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - User must have Administrator, Veterinarian, or Receptionist role
        /// - Database context must be initialized
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Index view with list of Owner objects if data exists
        /// - Returns Problem result if Owners DbSet is null
        /// - If no owners exist, returns empty list
        /// </post-condition>
        // GET: Owners
        public async Task<IActionResult> Index()
        {
            // Return all owners from database or Problem result if table is not accessible
            return _context.Owners != null ?
                        View(await _context.Owners.ToListAsync()) :
                        Problem("Entity set 'AnimalCareContext.Owners'  is null.");
        }

        /// <summary>
        /// Displays detailed information about a specific owner
        /// </summary>
        /// <param name="id">The unique identifier of the owner to display</param>
        /// <returns>View showing owner details</returns>
        /// <pre-condition>
        /// - User must be authenticated with valid role
        /// - Owner ID must be provided
        /// - Database must be accessible
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Details view if owner is found
        /// - Returns NotFound result if ID is null or owner doesn't exist
        /// </post-condition>
        // GET: Owners/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Validate that ID was provided
            if (id == null || _context.Owners == null)
            {
                return NotFound();
            }

            // Retrieve owner by ID
            var owner = await _context.Owners
                .FirstOrDefaultAsync(m => m.OwnerId == id);
            // Check if owner was found
            if (owner == null)
            {
                return NotFound();
            }

            return View(owner);
        }

        /// <summary>
        /// Displays the form for creating a new owner record
        /// </summary>
        /// <returns>View with empty owner form</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - User must have appropriate role permissions
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Create view with empty form
        /// </post-condition>
        // GET: Owners/Create
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Processes the form submission to create a new owner record
        /// </summary>
        /// <param name="owner">Owner object containing form data from the create view</param>
        /// <returns>Redirects to Index on success, returns Create view with errors on failure</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Owner object must contain valid data
        /// - Form must include valid anti-forgery token
        /// </pre-condition>
        /// <post-condition>
        /// - New owner record is saved to database on success
        /// - Redirects to Index view after successful creation
        /// - Returns Create view with validation errors if data is invalid
        /// </post-condition>
        // POST: Owners/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OwnerId,OwnerLastName,OwnerFirstName,OwnerPhone,OwnerEmail,OwnerAddress")] Owner owner)
        {
            if (ModelState.IsValid)
            {
                // Add new owner to database context
                _context.Add(owner);
                // Save changes to database
                await _context.SaveChangesAsync();
                // Redirect to Index page on success
                return RedirectToAction(nameof(Index));
            }
            // Return view with validation errors
            return View(owner);
        }

        /// <summary>
        /// Displays the form for editing an existing owner record
        /// </summary>
        /// <param name="id">The unique identifier of the owner to edit</param>
        /// <returns>View with populated owner form data</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Owner ID must be provided and valid
        /// - Owner must exist in database
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Edit view with owner data if found
        /// - Returns NotFound if ID is null or owner doesn't exist
        /// </post-condition>
        // GET: Owners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Validate ID parameter
            if (id == null || _context.Owners == null)
            {
                return NotFound();
            }

            // Find the owner by ID
            var owner = await _context.Owners.FindAsync(id);
            if (owner == null)
            {
                return NotFound();
            }
            return View(owner);
        }

        /// <summary>
        /// Processes the form submission to update an existing owner record
        /// </summary>
        /// <param name="id">The unique identifier of the owner to update</param>
        /// <param name="owner">Owner object containing updated form data</param>
        /// <returns>Redirects to Index on success, returns Edit view on failure</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Owner ID must match the ID in the owner object
        /// - Owner must exist in database
        /// - Form must include valid anti-forgery token
        /// </pre-condition>
        /// <post-condition>
        /// - Owner record is updated in database on success
        /// - Redirects to Index view after successful update
        /// - Returns NotFound if IDs don't match or owner doesn't exist
        /// - Returns Edit view with validation errors if data is invalid
        /// - Handles concurrency conflicts appropriately
        /// </post-condition>
        // POST: Owners/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OwnerId,OwnerLastName,OwnerFirstName,OwnerPhone,OwnerEmail,OwnerAddress")] Owner owner)
        {
            // Verify that route ID matches owner ID
            if (id != owner.OwnerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update owner in database context
                    _context.Update(owner);
                    // Save changes to database
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Check if owner still exists
                    if (!OwnerExists(owner.OwnerId))
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
            // Return view with validation errors
            return View(owner);
        }

        /// <summary>
        /// Displays confirmation page before deleting an owner record
        /// </summary>
        /// <param name="id">The unique identifier of the owner to delete</param>
        /// <returns>View showing owner details for deletion confirmation</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - Owner ID must be provided
        /// - Owner must exist in database
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Delete view with owner data if found
        /// - Returns NotFound if ID is null or owner doesn't exist
        /// </post-condition>
        // GET: Owners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Validate ID parameter
            if (id == null || _context.Owners == null)
            {
                return NotFound();
            }

            // Retrieve owner by ID
            var owner = await _context.Owners
                .FirstOrDefaultAsync(m => m.OwnerId == id);
            // Check if owner exists
            if (owner == null)
            {
                return NotFound();
            }

            return View(owner);
        }

        /// <summary>
        /// Processes the confirmed deletion of an owner record from the database
        /// </summary>
        /// <param name="id">The unique identifier of the owner to delete</param>
        /// <returns>Redirects to Index after successful deletion</returns>
        /// <pre-condition>
        /// - User must be authenticated with appropriate permissions
        /// - Owner ID must be valid
        /// - Form must include valid anti-forgery token
        /// - User must have confirmed deletion intent
        /// - Owner should not have associated animals (database constraints may prevent deletion)
        /// </pre-condition>
        /// <post-condition>
        /// - Owner record is permanently removed from database
        /// - Redirects to Index view after deletion
        /// - Returns Problem result if Owners DbSet is null
        /// - May fail if owner has associated animals due to foreign key constraints
        /// </post-condition>
        // POST: Owners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if Owners table is accessible
            if (_context.Owners == null)
            {
                return Problem("Entity set 'AnimalCareContext.Owners'  is null.");
            }
            // Find the owner by ID
            var owner = await _context.Owners.FindAsync(id);
            // Remove owner if found
            if (owner != null)
            {
                _context.Owners.Remove(owner);
            }

            // Save changes to database
            await _context.SaveChangesAsync();
            // Redirect to Index page
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Helper method to check if an owner exists in the database
        /// </summary>
        /// <param name="id">The unique identifier of the owner to check</param>
        /// <returns>True if owner exists, false otherwise</returns>
        /// <pre-condition>
        /// - Database context must be initialized
        /// - Owner ID must be provided
        /// </pre-condition>
        /// <post-condition>
        /// - Returns true if owner with given ID exists
        /// - Returns false if owner doesn't exist or Owners DbSet is null
        /// </post-condition>
        private bool OwnerExists(int id)
        {
            return (_context.Owners?.Any(e => e.OwnerId == id)).GetValueOrDefault();
        }
    }
}