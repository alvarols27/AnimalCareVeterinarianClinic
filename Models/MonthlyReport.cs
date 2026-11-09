using System;
using System.Collections.Generic;

namespace AnimalCareVeterinaryClinicDBFirst.Models;

/// <summary>
/// Represents a monthly report view entity in the veterinary clinic system
/// Read-only view that provides statistical data and metrics for clinic performance tracking
/// </summary>
public partial class MonthlyReport
{
    /// <summary>
    /// Gets or sets the month of the report
    /// </summary>
    /// <value>Month number (1-12) for which the report is generated</value>
    public int? Month { get; set; }

    /// <summary>
    /// Gets or sets the year of the report
    /// </summary>
    /// <value>Year for which the report is generated</value>
    public int? Year { get; set; }

    /// <summary>
    /// Gets or sets the total number of appointments for the month
    /// </summary>
    /// <value>Count of all appointments scheduled during the reporting period</value>
    public int? NumberOfAppointments { get; set; }

    /// <summary>
    /// Gets or sets the number of canceled appointments for the month
    /// </summary>
    /// <value>Count of appointments that were canceled during the reporting period</value>
    public int? NumberCanceled { get; set; }

    /// <summary>
    /// Gets or sets the number of active veterinarians for the month
    /// </summary>
    /// <value>Count of veterinarians who had appointments or were scheduled during the reporting period</value>
    public int NumberOfActiveVets { get; set; }

    /// <summary>
    /// Gets or sets the occupancy rate for the month
    /// </summary>
    /// <value>Percentage indicating clinic capacity utilization, calculated as decimal (e.g., 75.50 for 75.50%)</value>
    public decimal OccupancyRate { get; set; }
}