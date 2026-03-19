using System.ComponentModel.DataAnnotations;

namespace Payroll_Management_Solutions.Models
{
    public class Employees
    {
        [Key]
        public int EmployeeId { get; set; }
        public string? IdentityUserId { get; set; }
        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        [Required(ErrorMessage = "Salary is required")]
        [Range(1, 10000000)]
        public decimal BasicSalary { get; set; }
        [Required]
        public string? Role { get; set; }   // Admin / Employee
        public bool IsActive { get; set; }
        public bool NeedPasswordReset { get; set; }
    }
}
