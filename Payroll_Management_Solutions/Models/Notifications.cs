using System.ComponentModel.DataAnnotations;

namespace Payroll_Management_Solutions.Models
{
    public class Notifications
    {
        [Key]
        public int NotificationId { get; set; }//primary key
        public string Title { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }//-- Birthday, Holiday, Achievement
        public int CreatedBy { get; set; }//-- HR EmployeeId
        public bool IsForAll { get; set; }//-- Public holiday wishes
        public DateTime CreatedDate { get; set; }
    }
}
