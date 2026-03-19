namespace Payroll_Management_Solutions.Models.ViewModels
{
    public class EmployeeNotificationVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
    }

}
