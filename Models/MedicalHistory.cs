using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AnimalCareVeterinaryClinicDBFirst.Models;

/// <summary>
/// Represents a medical history record entity in the veterinary clinic system
/// Contains detailed information about veterinary consultations, diagnoses, and treatments for animals
/// </summary>
public partial class MedicalHistory
{
    /// <summary>
    /// Gets or sets the unique identifier for the medical history record
    /// </summary>
    /// <value>Primary key for the MedicalHistory entity</value>
    public int HistoryId { get; set; }

    /// <summary>
    /// Gets or sets the animal identifier for this medical history record
    /// </summary>
    /// <value>Foreign key referencing the Animal entity</value>
    public int AnimalId { get; set; }

    /// <summary>
    /// Gets or sets the date of the veterinary consultation
    /// </summary>
    /// <value>Date when the consultation took place</value>
    [Required]
    public DateTime ConsultationDate { get; set; }

    /// <summary>
    /// Gets or sets the observations made during the consultation
    /// </summary>
    /// <value>Detailed notes about the animal's condition and behavior during examination</value>
    [Required]
    public string? Observations { get; set; }

    /// <summary>
    /// Gets or sets the diagnosis determined by the veterinarian
    /// </summary>
    /// <value>Medical diagnosis or condition identified</value>
    [Required]
    public string? Diagnosis { get; set; }

    /// <summary>
    /// Gets or sets the treatment prescribed or administered
    /// </summary>
    /// <value>Treatment plan, medications, or procedures recommended or performed</value>
    [Required]
    public string? Treatment { get; set; }

    /// <summary>
    /// Gets or sets the veterinarian identifier who conducted the consultation
    /// </summary>
    /// <value>Foreign key referencing the Veterinarian entity</value>
    public int VetId { get; set; }

    /// <summary>
    /// Gets or sets the animal associated with this medical history record
    /// </summary>
    /// <value>Navigation property to the Animal entity</value>
    public virtual Animal Animal { get; set; } = null!;

    /// <summary>
    /// Gets or sets the veterinarian who conducted the consultation
    /// </summary>
    /// <value>Navigation property to the Veterinarian entity who provided the care</value>
    public virtual Veterinarian Vet { get; set; } = null!;
}