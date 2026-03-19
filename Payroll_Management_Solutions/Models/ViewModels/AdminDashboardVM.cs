namespace Payroll_Management_Solutions.Models.ViewModels
{
    public class AdminDashboardVM
    {
        public int TotalEmployees { get; set; }
        public int PendingApprovals { get; set; }
        public int HrMembers { get; set; }
        public IList<Notifications> TodayEvents { get; set; } = new List<Notifications>();
    }
}

