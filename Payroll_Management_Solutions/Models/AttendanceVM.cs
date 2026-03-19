namespace Payroll_Management_Solutions.Models
{
    public class AttendanceVM
    {
        public int EmployeeId { get; set; }
        public string Status { get; set; }
        public TimeSpan? InTime { get; set; }
        public TimeSpan? OutTime { get; set; }
    }

}
