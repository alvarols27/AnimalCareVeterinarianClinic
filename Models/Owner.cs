using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AnimalCareVeterinaryClinicDBFirst.Models;

/// <summary>
/// Represents a pet owner entity in the veterinary clinic system
/// Contains contact information and personal details for clients who own animals
/// </summary>
public partial class Owner
{
    /// <summary>
    /// Gets or sets the unique identifier for the owner
    /// </summary>
    /// <value>Primary key for the Owner entity</value>
    public int OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the last name of the owner
    /// </summary>
    /// <value>Owner's last name, limited to 50 characters and containing only letters and spaces</value>
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[A-Za-z\s]+$",
    ErrorMessage = "Name can only contain letters and spaces.")]
    public string? OwnerLastName { get; set; }

    /// <summary>
    /// Gets or sets the first name of the owner
    /// </summary>
    /// <value>Owner's first name, limited to 50 characters and containing only letters and spaces</value>
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[A-Za-z\s]+$",
    ErrorMessage = "Name can only contain letters and spaces.")]
    public string? OwnerFirstName { get; set; }

    /// <summary>
    /// Gets or sets the phone number of the owner
    /// </summary>
    /// <value>Owner's contact phone number in valid North American format</value>
    [Required]
    [RegularExpression(@"^(\+?1\s?)?(\(?\d{3}\)?[\s.-]?)\d{3}[\s.-]?\d{4}$",
    ErrorMessage = "Enter a valid phone number.")]
    public string? OwnerPhone { get; set; }

    /// <summary>
    /// Gets or sets the email address of the owner
    /// </summary>
    /// <value>Owner's email address, must be a valid .com email format</value>
    [Required(ErrorMessage = "Email is required.")]
    [RegularExpression(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.com$",
    ErrorMessage = "Please enter a valid .com email address.")]
    public string? OwnerEmail { get; set; }

    /// <summary>
    /// Gets or sets the physical address of the owner
    /// </summary>
    /// <value>Owner's street address in valid format starting with a number</value>
    [Required(ErrorMessage = "Address is required.")]
    [RegularExpression(
     @"^[0-9]+[A-Za-zÀ-ÖØ-öø-ÿ\-]*\s+[A-Za-zÀ-ÖØ-öø-ÿ0-9\.\-'\s,]+$",
     ErrorMessage = "Please enter a valid address."
 )]
    public string? OwnerAddress { get; set; }

    /// <summary>
    /// Gets or sets the collection of animals owned by this owner
    /// </summary>
    /// <value>Navigation property containing all animals belonging to this owner</value>
    public virtual ICollection<Animal> Animals { get; set; } = new List<Animal>();
}