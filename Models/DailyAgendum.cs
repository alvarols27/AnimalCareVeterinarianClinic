using System;
using System.Collections.Generic;

namespace AnimalCareVeterinaryClinicDBFirst.Models;

/// <summary>
/// Represents a daily agenda view entity in the veterinary clinic system
/// Read-only view that displays the daily schedule of appointments for veterinarians
/// </summary>
public partial class DailyAgendum
{
    /// <summary>
    /// Gets or sets the date of the appointment
    /// </summary>
    /// <value>Date when the appointment is scheduled</value>
    public DateTime? Date { get; set; }

    /// <summary>
    /// Gets or sets the name of the veterinarian assigned to the appointment
    /// </summary>
    /// <value>Full name of the veterinarian in "LastName FirstName" format</value>
    public string? VeterinarianName { get; set; }

    /// <summary>
    /// Gets or sets the start time of the appointment
    /// </summary>
    /// <value>Time when the appointment begins</value>
    public TimeSpan? StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the appointment
    /// </summary>
    /// <value>Time when the appointment ends</value>
    public TimeSpan? EndTime { get; set; }

    /// <summary>
    /// Gets or sets the name of the animal for the appointment
    /// </summary>
    /// <value>Name of the pet being seen</value>
    public string? AnimalName { get; set; }

    /// <summary>
    /// Gets or sets the name of the pet owner
    /// </summary>
    /// <value>Full name of the owner in "FirstName LastName" format</value>
    public string? OwnerName { get; set; }

    /// <summary>
    /// Gets or sets the status of the appointment
    /// </summary>
    /// <value>Current status (e.g., Scheduled, Completed, Canceled)</value>
    public string? AppointmentStatus { get; set; }
}