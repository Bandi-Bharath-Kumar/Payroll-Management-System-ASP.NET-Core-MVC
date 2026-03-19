namespace Payroll_Management_Solutions.Models.ViewModels
{
    public class EmployeeDashboardVM
    {
        // Attendance
        public List<string> AttendanceLabels { get; set; }
        public List<int> AttendanceCounts { get; set; }

        // Payroll
        public List<string> PayrollMonths { get; set; }
        public List<decimal> PayrollAmounts { get; set; }
    }
}
