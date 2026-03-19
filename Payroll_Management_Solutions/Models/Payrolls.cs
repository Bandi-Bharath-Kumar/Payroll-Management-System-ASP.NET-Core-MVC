//PerDaySalary = BasicSalary / TotalWorkingDays
//PayableDays = PresentDays
//GrossSalary = PerDaySalary * PayableDays
//NetSalary = GrossSalary - Deductions


using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Payroll_Management_Solutions.Models
{
    public class Payrolls
    {
        [Key]
        public int PayrollId { get; set; }

        //[Required]
        //public int EmployeeId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please select an employee")]
        public int EmployeeId { get; set; }


        [ForeignKey("EmployeeId")]
        [ValidateNever]   // ✅ THIS FIXES THE ISSUE
        public Employees? Employee { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        public decimal BasicSalary { get; set; }

        //[Required]
        //public int TotalWorkingDays { get; set; }

        //[Required]
        //public int PresentDays { get; set; }
        [Range(1, 31, ErrorMessage = "Total Working Days must be between 1 and 31")]
        public int TotalWorkingDays { get; set; }

        [Range(0, 31, ErrorMessage = "Present Days must be between 0 and 31")]
        public int PresentDays { get; set; }

        public decimal GrossSalary { get; set; }

        public decimal Deductions { get; set; }

        public decimal NetSalary { get; set; }

        //NEW COLUMNS
        public bool IsApproved { get; set; } = false;
        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedOn { get; set; }
    }
}
