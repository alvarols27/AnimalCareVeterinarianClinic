using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AnimalCareVeterinaryClinicDBFirst.Models;

/// <summary>
/// Represents a user role entity in the veterinary clinic system
/// Defines different access levels and permissions for users (e.g., Administrator, Veterinarian, Receptionist)
/// </summary>
public partial class Role
{
    /// <summary>
    /// Gets or sets the unique identifier for the role
    /// </summary>
    /// <value>Primary key for the Role entity</value>
    public int RoleId { get; set; }

    /// <summary>
    /// Gets or sets the name of the role
    /// </summary>
    /// <value>Role name, limited to 50 characters and containing only letters and spaces</value>
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[A-Za-z\s]+$",
   ErrorMessage = "Role can only contain letters and spaces.")]
    public string RoleName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of user accounts assigned to this role
    /// </summary>
    /// <value>Navigation property containing all user accounts with this role</value>
    public virtual ICollection<UserAccount> UserAccounts { get; set; } = new List<UserAccount>();
}