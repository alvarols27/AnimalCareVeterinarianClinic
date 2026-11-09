namespace AnimalCareVeterinaryClinicDBFirst.Models
{
    /// <summary>
    /// View model for updating medical notes after an appointment
    /// Combines appointment details with medical history information for efficient data entry
    /// </summary>
    public class UpdateMedicalNotes
    {
        /// <summary>
        /// Gets or sets the appointment identifier being updated
        /// </summary>
        /// <value>Unique identifier of the appointment for which medical notes are being recorded</value>
        public int AppointmentId { get; set; }

        /// <summary>
        /// Gets or sets the status of the appointment
        /// </summary>
        /// <value>Current appointment status (e.g., Completed, In Progress)</value>
        public string AppointmentStatus { get; set; }

        /// <summary>
        /// Gets or sets the observations made during the appointment
        /// </summary>
        /// <value>Clinical observations and notes recorded by the veterinarian during examination</value>
        public string Observations { get; set; }

        /// <summary>
        /// Gets or sets the diagnosis determined during the appointment
        /// </summary>
        /// <value>Medical diagnosis or condition identified by the veterinarian</value>
        public string Diagnosis { get; set; }

        /// <summary>
        /// Gets or sets the treatment prescribed or administered
        /// </summary>
        /// <value>Treatment plan, medications, or procedures recommended or performed</value>
        public string Treatment { get; set; }

        /// <summary>
        /// Gets or sets the name of the animal seen during the appointment
        /// </summary>
        /// <value>Name of the pet for display purposes</value>
        public string AnimalName { get; set; }

        /// <summary>
        /// Gets or sets the species of the animal
        /// </summary>
        /// <value>Animal species for context and reference</value>
        public string Species { get; set; }
    }
}