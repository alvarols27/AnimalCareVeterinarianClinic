namespace AnimalCareVeterinaryClinicDBFirst.Models
{
    public class NotificationService
    {


        private readonly AnimalCareContext _context;

        public NotificationService(AnimalCareContext context)
        {
            _context = context;
        }

        public async Task Create(int appointmentId, string type, string status = "Pending", string channel = "Reminder")
        {
            var notification = new Notification
            {
                AppointmentId = appointmentId,
                NotificationType = type,
                NotificationStatus = status,
                Channel = channel,
              
                SendDate = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}
