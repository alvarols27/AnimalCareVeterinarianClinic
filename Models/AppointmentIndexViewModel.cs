namespace AnimalCareVeterinaryClinicDBFirst.Models
{
    /// <summary>
    /// View model for displaying appointments organized by time period on the index page
    /// Categorizes appointments into today's, upcoming, and past appointments for better organization
    /// </summary>
    public class AppointmentIndexViewModel
    {
        /// <summary>
        /// Gets or sets the list of appointments scheduled for today
        /// </summary>
        /// <value>Collection of appointments occurring on the current date</value>
        public List<Appointment> Today { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of upcoming appointments scheduled for future dates
        /// </summary>
        /// <value>Collection of appointments occurring after today</value>
        public List<Appointment> Upcoming { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of past appointments that have already occurred
        /// </summary>
        /// <value>Collection of appointments that occurred before today</value>
        public List<Appointment> Past { get; set; } = new();
    }
}