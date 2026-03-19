using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Payroll_Management_Solutions.Models
{
    // Declare enum outside the class
    public enum AttendanceStatus
    {
        Present,
        Absent,
        Leave
    }
    public class Attendances
    {
        [Key]
        public int AttendanceId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employees Employee { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public  AttendanceStatus Status { get; set; } // Present / Absent / Leave

        public TimeSpan? InTime { get; set; }
        public TimeSpan? OutTime { get; set; }
            
        public double WorkingHours { get; set; }
    }
}
