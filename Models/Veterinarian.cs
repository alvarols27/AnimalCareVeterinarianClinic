using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AnimalCareVeterinaryClinicDBFirst.Models;

/// <summary>
/// Represents a veterinarian entity in the veterinary clinic system
/// Contains professional information and credentials for veterinary staff members
/// </summary>
public partial class Veterinarian
{
    /// <summary>
    /// Gets or sets the unique identifier for the veterinarian
    /// </summary>
    /// <value>Primary key for the Veterinarian entity</value>
    public int VetId { get; set; }

    /// <summary>
    /// Gets or sets the last name of the veterinarian
    /// </summary>
    /// <value>Veterinarian's last name, limited to 50 characters and containing only letters and spaces</value>
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[A-Za-z\s]+$",
     ErrorMessage = "Name can only contain letters and spaces.")]
    public string? VetLastName { get; set; }

    /// <summary>
    /// Gets or sets the first name of the veterinarian
    /// </summary>
    /// <value>Veterinarian's first name, limited to 50 characters and containing only letters and spaces</value>
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[A-Za-z\s]+$",
    ErrorMessage = "Name can only contain letters and spaces.")]
    public string? VetFirstName { get; set; }

    /// <summary>
    /// Gets or sets the medical specialty of the veterinarian
    /// </summary>
    /// <value>Veterinarian's area of specialization, limited to 50 characters and containing only letters and spaces</value>
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[A-Za-z\s]+$",
    ErrorMessage = "Specialty can only contain letters and spaces.")]
    public string? VetSpecialty { get; set; }

    /// <summary>
    /// Gets or sets the phone number of the veterinarian
    /// </summary>
    /// <value>Veterinarian's contact phone number, must be between 7-20 characters and contain valid phone number characters</value>
    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^[0-9\s\-\(\)\+]{7,20}$",
     ErrorMessage = "Please enter a valid phone number.")]
    public string? VetPhone { get; set; }

    /// <summary>
    /// Gets or sets the email address of the veterinarian
    /// </summary>
    /// <value>Veterinarian's professional email address, must be a valid .com email format</value>
    [Required(ErrorMessage = "Email is required.")]
    [RegularExpression(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.com$",
   ErrorMessage = "Please enter a valid .com email address.")]
    public string? VetEmail { get; set; }

    /// <summary>
    /// Gets or sets the collection of appointments assigned to this veterinarian
    /// </summary>
    /// <value>Navigation property containing all appointments scheduled with this veterinarian</value>
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    /// <summary>
    /// Gets or sets the collection of medical history records created by this veterinarian
    /// </summary>
    /// <value>Navigation property containing all medical consultations conducted by this veterinarian</value>
    public virtual ICollection<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();

    /// <summary>
    /// Gets or sets the collection of schedules for this veterinarian
    /// </summary>
    /// <value>Navigation property containing all work schedules and availability for this veterinarian</value>
    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}