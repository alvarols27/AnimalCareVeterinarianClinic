using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimalCareVeterinaryClinicDBFirst.Models;

/// <summary>
/// Represents a schedule entity in the veterinary clinic system
/// Contains availability and working hours information for veterinarians
/// </summary>
public partial class Schedule
{
    /// <summary>
    /// Gets or sets the unique identifier for the schedule
    /// </summary>
    /// <value>Primary key for the Schedule entity</value>
    public int ScheduleId { get; set; }

    /// <summary>
    /// Gets or sets the veterinarian identifier for this schedule
    /// </summary>
    /// <value>Foreign key referencing the Veterinarian entity</value>
    [Required]
    public int VetId { get; set; }

    /// <summary>
    /// Gets or sets the date of the schedule
    /// </summary>
    /// <value>Date for which this schedule applies</value>
    [Required]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime ScheduleDate { get; set; }

    /// <summary>
    /// Gets or sets the start time of the schedule
    /// </summary>
    /// <value>Time when the veterinarian's work period begins</value>
    [Required]
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the schedule
    /// </summary>
    /// <value>Time when the veterinarian's work period ends</value>
    [Required]
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Gets or sets the status of the schedule
    /// </summary>
    /// <value>Status indicator (e.g., Free, Busy, Unavailable)</value>
    public string? ScheduleStatus { get; set; }

    /// <summary>
    /// Gets or sets the veterinarian associated with this schedule
    /// </summary>
    /// <value>Navigation property to the Veterinarian entity</value>
    public virtual Veterinarian Vet { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether the veterinarian is currently busy with an appointment
    /// </summary>
    /// <value>True if the veterinarian has an active appointment right now; otherwise, false. Not stored in database.</value>
    [NotMapped]
    public bool IsVetBusyNow { get; set; }
}