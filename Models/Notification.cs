using System;
using System.Collections.Generic;

namespace AnimalCareVeterinaryClinicDBFirst.Models;

/// <summary>
/// Represents a notification entity in the veterinary clinic system
/// Contains information about reminders and communications sent for appointments
/// </summary>
public partial class Notification
{
    /// <summary>
    /// Gets or sets the unique identifier for the notification
    /// </summary>
    /// <value>Primary key for the Notification entity</value>
    public int NotificationId { get; set; }

    /// <summary>
    /// Gets or sets the appointment identifier associated with this notification
    /// </summary>
    /// <value>Foreign key referencing the Appointment entity</value>
    public int AppointmentId { get; set; }

    /// <summary>
    /// Gets or sets the type of notification
    /// </summary>
    /// <value>Notification category (e.g., Reminder, Confirmation, Cancellation)</value>
    public string? NotificationType { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the notification should be or was sent
    /// </summary>
    /// <value>Scheduled or actual send DateTime for the notification</value>
    public DateTime? SendDate { get; set; }

    /// <summary>
    /// Gets or sets the communication channel used for the notification
    /// </summary>
    /// <value>Delivery method (e.g., Email, SMS, Phone)</value>
    public string? Channel { get; set; }

    /// <summary>
    /// Gets or sets the current status of the notification
    /// </summary>
    /// <value>Status indicator (e.g., Pending, Sent, Failed)</value>
    public string? NotificationStatus { get; set; }

    /// <summary>
    /// Gets or sets the appointment associated with this notification
    /// </summary>
    /// <value>Navigation property to the Appointment entity for which this notification was created</value>
    public virtual Appointment Appointment { get; set; } = null!;
}