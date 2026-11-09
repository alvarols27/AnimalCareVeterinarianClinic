using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AnimalCareVeterinaryClinicDBFirst.Models;

/// <summary>
/// Represents a user account entity in the veterinary clinic system
/// Contains authentication credentials and personal information for system users
/// </summary>
public partial class UserAccount
{
    /// <summary>
    /// Gets or sets the unique identifier for the user account
    /// </summary>
    /// <value>Primary key for the UserAccount entity</value>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the login username for authentication
    /// </summary>
    /// <value>Unique username used to log into the system</value>
    public string UserLogin { get; set; } = null!;

    /// <summary>
    /// Gets or sets the password for authentication
    /// </summary>
    /// <value>User's password for system access</value>
    public string UserPassword { get; set; } = null!;

    /// <summary>
    /// Gets or sets the first name of the user
    /// </summary>
    /// <value>User's first name, limited to 50 characters and containing only letters and spaces</value>
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[A-Za-z\s]+$",
   ErrorMessage = "First Name can only contain letters and spaces.")]
    public string? UserFirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the user
    /// </summary>
    /// <value>User's last name, limited to 50 characters and containing only letters and spaces</value>
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[A-Za-z\s]+$",
   ErrorMessage = "Last Name can only contain letters and spaces.")]
    public string? UserLastName { get; set; }

    /// <summary>
    /// Gets or sets the role identifier for this user account
    /// </summary>
    /// <value>Foreign key referencing the Role entity that determines user permissions</value>
    public int RoleId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user account is active
    /// </summary>
    /// <value>True if the account is active and can log in; false if the account is disabled</value>
    public bool? UserActive { get; set; }

    /// <summary>
    /// Gets or sets the role assigned to this user account
    /// </summary>
    /// <value>Navigation property to the Role entity that defines access permissions</value>
    public virtual Role Role { get; set; } = null!;
}