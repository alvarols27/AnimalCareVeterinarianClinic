using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AnimalCareVeterinaryClinicDBFirst.Models;

/// <summary>
/// Represents an appointment entity in the veterinary clinic system
/// Contains information about scheduled visits between animals and veterinarians
/// </summary>
public partial class Appointment
{
    /// <summary>
    /// Gets or sets the unique identifier for the appointment
    /// </summary>
    /// <value>Primary key for the Appointment entity</value>
    public int AppointmentId { get; set; }

    /// <summary>
    /// Gets or sets the veterinarian identifier for the appointment
    /// </summary>
    /// <value>Foreign key referencing the Veterinarian entity</value>
    [Required(ErrorMessage = "Please select a veterinarian")]
    public int VetId { get; set; }

    /// <summary>
    /// Gets or sets the animal identifier for the appointment
    /// </summary>
    /// <value>Foreign key referencing the Animal entity</value>
    [Required(ErrorMessage = "Please select an animal")]
    public int AnimalId { get; set; }

    /// <summary>
    /// Gets or sets the start date and time of the appointment
    /// </summary>
    /// <value>DateTime when the appointment begins</value>
    [Required(ErrorMessage = "Please select a start time")]
    public DateTime StartDateTime { get; set; }

    /// <summary>
    /// Gets or sets the end date and time of the appointment
    /// </summary>
    /// <value>DateTime when the appointment ends</value>
    [Required(ErrorMessage = "Please select an end time")]
    public DateTime EndDateTime { get; set; }

    /// <summary>
    /// Gets or sets the purpose or reason for the appointment
    /// </summary>
    /// <value>Description of the appointment purpose (e.g., Checkup, Vaccination, Surgery)</value>
    [Required(ErrorMessage = "Please select a purpose")]
    public string? Purpose { get; set; }

    /// <summary>
    /// Gets or sets the current status of the appointment
    /// </summary>
    /// <value>Status indicator (e.g., Scheduled, Completed, Canceled)</value>
    public string? AppointmentStatus { get; set; }

    /// <summary>
    /// Gets or sets the animal associated with this appointment
    /// </summary>
    /// <value>Navigation property to the Animal entity being seen</value>
    public virtual Animal Animal { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of notifications associated with this appointment
    /// </summary>
    /// <value>Navigation property containing all notifications (reminders, confirmations) for this appointment</value>
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    /// <summary>
    /// Gets or sets the veterinarian assigned to this appointment
    /// </summary>
    /// <value>Navigation property to the Veterinarian entity conducting the appointment</value>
    public virtual Veterinarian Vet { get; set; } = null!;
}