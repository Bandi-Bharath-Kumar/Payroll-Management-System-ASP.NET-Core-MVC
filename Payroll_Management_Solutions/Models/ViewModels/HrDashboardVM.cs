namespace Payroll_Management_Solutions.Models.ViewModels
{
    public class HrDashboardVM
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public List<Employees> Employees { get; set; } = new List<Employees>();
    }
}

