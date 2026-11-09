using System.ComponentModel.DataAnnotations;

namespace AnimalCareVeterinaryClinicDBFirst.Models
{
    /// <summary>
    /// View model for user authentication on the login page
    /// Contains credentials and validation rules for user login
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Gets or sets the username for authentication
        /// </summary>
        /// <value>User's login username</value>
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password for authentication
        /// </summary>
        /// <value>User's login password, masked in the UI</value>
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message to display when login fails
        /// </summary>
        /// <value>Error message text for invalid credentials or authentication failures</value>
        public string? ErrorMessage { get; set; }
    }
}