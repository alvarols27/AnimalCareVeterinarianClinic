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
    /// Controller responsible for managing user role records in the veterinary clinic system
    /// Handles CRUD operations for roles and enforces administrator-only access control
    /// </summary>
    [Authorize(Roles = "Administrator")]
    public class RolesController : Controller
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
        /// - Controller is ready to handle role-related requests
        /// </post-condition>
        public RolesController(AnimalCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all user roles from the database
        /// </summary>
        /// <returns>View displaying all roles in a table format</returns>
        /// <pre-condition>
        /// - User must be authenticated
        /// - User must have Administrator role
        /// - Database context must be initialized
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Index view with list of Role objects if data exists
        /// - Returns Problem result if Roles DbSet is null
        /// - If no roles exist, returns empty list
        /// </post-condition>
        // GET: Roles
        public async Task<IActionResult> Index()
        {
            // Return all roles from database or Problem result if table is not accessible
            return _context.Roles != null ?
                        View(await _context.Roles.ToListAsync()) :
                        Problem("Entity set 'AnimalCareContext.Roles'  is null.");
        }

        /// <summary>
        /// Displays detailed information about a specific role
        /// </summary>
        /// <param name="id">The unique identifier of the role to display</param>
        /// <returns>View showing role details</returns>
        /// <pre-condition>
        /// - User must be authenticated as Administrator
        /// - Role ID must be provided
        /// - Database must be accessible
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Details view if role is found
        /// - Returns NotFound result if ID is null or role doesn't exist
        /// </post-condition>
        // GET: Roles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Validate that ID was provided
            if (id == null || _context.Roles == null)
            {
                return NotFound();
            }

            // Retrieve role by ID
            var role = await _context.Roles
                .FirstOrDefaultAsync(m => m.RoleId == id);
            // Check if role was found
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        /// <summary>
        /// Displays the form for creating a new role record
        /// </summary>
        /// <returns>View with empty role form</returns>
        /// <pre-condition>
        /// - User must be authenticated as Administrator
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Create view with empty form
        /// </post-condition>
        // GET: Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Processes the form submission to create a new role record
        /// </summary>
        /// <param name="role">Role object containing form data from the create view</param>
        /// <returns>Redirects to Index on success, returns Create view with errors on failure</returns>
        /// <pre-condition>
        /// - User must be authenticated as Administrator
        /// - Role object must contain valid data
        /// - Form must include valid anti-forgery token
        /// </pre-condition>
        /// <post-condition>
        /// - New role record is saved to database on success
        /// - Redirects to Index view after successful creation
        /// - Returns Create view with validation errors if data is invalid
        /// </post-condition>
        // POST: Roles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoleId,RoleName")] Role role)
        {
            if (ModelState.IsValid)
            {
                // Add new role to database context
                _context.Add(role);
                // Save changes to database
                await _context.SaveChangesAsync();
                // Redirect to Index page on success
                return RedirectToAction(nameof(Index));
            }
            // Return view with validation errors
            return View(role);
        }

        /// <summary>
        /// Displays the form for editing an existing role record
        /// </summary>
        /// <param name="id">The unique identifier of the role to edit</param>
        /// <returns>View with populated role form data</returns>
        /// <pre-condition>
        /// - User must be authenticated as Administrator
        /// - Role ID must be provided and valid
        /// - Role must exist in database
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Edit view with role data if found
        /// - Returns NotFound if ID is null or role doesn't exist
        /// </post-condition>
        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Validate ID parameter
            if (id == null || _context.Roles == null)
            {
                return NotFound();
            }

            // Find the role by ID
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        /// <summary>
        /// Processes the form submission to update an existing role record
        /// </summary>
        /// <param name="id">The unique identifier of the role to update</param>
        /// <param name="role">Role object containing updated form data</param>
        /// <returns>Redirects to Index on success, returns Edit view on failure</returns>
        /// <pre-condition>
        /// - User must be authenticated as Administrator
        /// - Role ID must match the ID in the role object
        /// - Role must exist in database
        /// - Form must include valid anti-forgery token
        /// </pre-condition>
        /// <post-condition>
        /// - Role record is updated in database on success
        /// - Redirects to Index view after successful update
        /// - Returns NotFound if IDs don't match or role doesn't exist
        /// - Returns Edit view with validation errors if data is invalid
        /// - Handles concurrency conflicts appropriately
        /// </post-condition>
        // POST: Roles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoleId,RoleName")] Role role)
        {
            // Verify that route ID matches role ID
            if (id != role.RoleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update role in database context
                    _context.Update(role);
                    // Save changes to database
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Check if role still exists
                    if (!RoleExists(role.RoleId))
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
            return View(role);
        }

        /// <summary>
        /// Displays confirmation page before deleting a role record
        /// </summary>
        /// <param name="id">The unique identifier of the role to delete</param>
        /// <returns>View showing role details for deletion confirmation</returns>
        /// <pre-condition>
        /// - User must be authenticated as Administrator
        /// - Role ID must be provided
        /// - Role must exist in database
        /// </pre-condition>
        /// <post-condition>
        /// - Returns Delete view with role data if found
        /// - Returns NotFound if ID is null or role doesn't exist
        /// </post-condition>
        // GET: Roles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Validate ID parameter
            if (id == null || _context.Roles == null)
            {
                return NotFound();
            }

            // Retrieve role by ID
            var role = await _context.Roles
                .FirstOrDefaultAsync(m => m.RoleId == id);
            // Check if role exists
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        /// <summary>
        /// Processes the confirmed deletion of a role record from the database
        /// </summary>
        /// <param name="id">The unique identifier of the role to delete</param>
        /// <returns>Redirects to Index after successful deletion</returns>
        /// <pre-condition>
        /// - User must be authenticated as Administrator
        /// - Role ID must be valid
        /// - Form must include valid anti-forgery token
        /// - User must have confirmed deletion intent
        /// - Role should not have associated user accounts (database constraints may prevent deletion)
        /// </pre-condition>
        /// <post-condition>
        /// - Role record is permanently removed from database
        /// - Redirects to Index view after deletion
        /// - Returns Problem result if Roles DbSet is null
        /// - May fail if role has associated user accounts due to foreign key constraints
        /// </post-condition>
        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if Roles table is accessible
            if (_context.Roles == null)
            {
                return Problem("Entity set 'AnimalCareContext.Roles'  is null.");
            }
            // Find the role by ID
            var role = await _context.Roles.FindAsync(id);
            // Remove role if found
            if (role != null)
            {
                _context.Roles.Remove(role);
            }

            // Save changes to database
            await _context.SaveChangesAsync();
            // Redirect to Index page
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Helper method to check if a role exists in the database
        /// </summary>
        /// <param name="id">The unique identifier of the role to check</param>
        /// <returns>True if role exists, false otherwise</returns>
        /// <pre-condition>
        /// - Database context must be initialized
        /// - Role ID must be provided
        /// </pre-condition>
        /// <post-condition>
        /// - Returns true if role with given ID exists
        /// - Returns false if role doesn't exist or Roles DbSet is null
        /// </post-condition>
        private bool RoleExists(int id)
        {
            return (_context.Roles?.Any(e => e.RoleId == id)).GetValueOrDefault();
        }
    }
}