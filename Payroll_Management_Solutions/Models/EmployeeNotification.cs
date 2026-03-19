using System.ComponentModel.DataAnnotations;

namespace Payroll_Management_Solutions.Models
{
    public class EmployeeNotifications
    {
        [Key]
        public int EmployeeNotificationId { get; set; }

        // FK → Employees
        public int EmployeeId { get; set; }
        public Employees Employee { get; set; }

        // FK → Notifications
        public int NotificationId { get; set; }
        public Notifications Notification { get; set; }

        public bool IsRead { get; set; }
    }
}
