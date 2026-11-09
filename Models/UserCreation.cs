namespace AnimalCareVeterinaryClinicDBFirst.Models
{
    /// <summary>
    /// View model for creating new user accounts with optional veterinarian profile information
    /// Combines user account data with veterinarian-specific fields for streamlined account creation
    /// </summary>
    public class UserCreation
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user account
        /// </summary>
        /// <value>Primary key for the UserAccount entity, auto-generated on creation</value>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the login username for authentication
        /// </summary>
        /// <value>Unique username that will be used to log into the system</value>
        public string UserLogin { get; set; } = null!;

        /// <summary>
        /// Gets or sets the password for authentication
        /// </summary>
        /// <value>User's password for system access</value>
        public string UserPassword { get; set; } = null!;

        /// <summary>
        /// Gets or sets the first name of the user
        /// </summary>
        /// <value>User's first name</value>
        public string? UserFirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the user
        /// </summary>
        /// <value>User's last name</value>
        public string? UserLastName { get; set; }

        /// <summary>
        /// Gets or sets the role identifier for this user account
        /// </summary>
        /// <value>Foreign key referencing the Role entity that determines user permissions</value>
        public int RoleId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user account is active
        /// </summary>
        /// <value>True if the account is active and can log in; false if the account is disabled</value>
        public bool? UserActive { get; set; }

        /// <summary>
        /// Gets or sets the role assigned to this user account
        /// </summary>
        /// <value>Navigation property to the Role entity</value>
        public virtual Role Role { get; set; } = null!;

        // Vet fields
        /// <summary>
        /// Gets or sets the veterinary specialty (only applicable when RoleId is Veterinarian)
        /// </summary>
        /// <value>Veterinarian's area of specialization (e.g., Surgery, Cardiology, Dermatology)</value>
        public string? VetSpecialty { get; set; }

        /// <summary>
        /// Gets or sets the veterinarian's phone number (only applicable when RoleId is Veterinarian)
        /// </summary>
        /// <value>Contact phone number for the veterinarian</value>
        public string? VetPhone { get; set; }

        /// <summary>
        /// Gets or sets the veterinarian's email address (only applicable when RoleId is Veterinarian)
        /// </summary>
        /// <value>Professional email address for the veterinarian</value>
        public string? VetEmail { get; set; }
    }
}