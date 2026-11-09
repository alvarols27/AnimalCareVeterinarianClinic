namespace AnimalCareVeterinaryClinicDBFirst.Models
{
    /// <summary>
    /// View model for displaying error information on the error page
    /// Contains request tracking information for debugging and error logging purposes
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the HTTP request that resulted in an error
        /// </summary>
        /// <value>Request ID used for tracking and correlating errors in logs</value>
        public string? RequestId { get; set; }

        /// <summary>
        /// Gets a value indicating whether the request ID should be displayed
        /// </summary>
        /// <value>True if RequestId is not null or empty; otherwise, false</value>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}