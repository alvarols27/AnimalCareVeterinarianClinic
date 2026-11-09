using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AnimalCareVeterinaryClinicDBFirst.Models;

/// <summary>
/// Represents an animal entity in the veterinary clinic system
/// Contains information about pets including their physical characteristics and relationships to owners
/// </summary>
public partial class Animal
{
    /// <summary>
    /// Gets or sets the unique identifier for the animal
    /// </summary>
    /// <value>Primary key for the Animal entity</value>
    public int AnimalId { get; set; }


    /// <summary>
    /// Gets or sets the name of the animal
    /// </summary>
    /// <value>Animal's name, limited to 50 characters and containing only letters and spaces</value>
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[A-Za-z\s]+$",
   ErrorMessage = "Name can only contain letters and spaces.")]
    public string? AnimalName { get; set; }

    /// <summary>
    /// Gets or sets the species of the animal
    /// </summary>
    /// <value>Animal's species (e.g., Dog, Cat, Bird)</value>
    [Required]
    public string? Species { get; set; }

    /// <summary>
    /// Gets or sets the breed of the animal
    /// </summary>
    /// <value>Animal's breed or mixed breed designation</value>
    [Required]
    public string? Breed { get; set; }

    /// <summary>
    /// Gets or sets the sex of the animal
    /// </summary>
    /// <value>Animal's biological sex (e.g., Male, Female)</value>
    [Required]
    public string? Sex { get; set; }

    /// <summary>
    /// Gets or sets the birth date of the animal
    /// </summary>
    /// <value>Animal's date of birth for age calculation</value>
    [Required]
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Gets or sets the weight of the animal
    /// </summary>
    /// <value>Animal's weight in decimal format for medical tracking</value>
    [Required]
    public decimal? AnimalWeight { get; set; }

    /// <summary>
    /// Gets or sets the owner identifier for the animal
    /// </summary>
    /// <value>Foreign key referencing the Owner entity</value>
    public int OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the collection of appointments associated with this animal
    /// </summary>
    /// <value>Navigation property containing all appointments for this animal</value>
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    /// <summary>
    /// Gets or sets the collection of medical history records for this animal
    /// </summary>
    /// <value>Navigation property containing all medical history entries for this animal</value>
    public virtual ICollection<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();

    /// <summary>
    /// Gets or sets the owner of this animal
    /// </summary>
    /// <value>Navigation property to the Owner entity who owns this animal</value>
    public virtual Owner Owner { get; set; } = null!;
}