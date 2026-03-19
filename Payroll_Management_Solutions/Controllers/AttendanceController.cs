using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payroll_Management_Solutions.Data;
using Payroll_Management_Solutions.Models;
using System.Linq;

namespace Payroll_Management_Solutions.Controllers
{
    [Authorize(Roles = "Admin,HR,Employee")]
    public class AttendanceController : Controller
    {
        private readonly PayrollDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        //public AttendanceController(PayrollDbContext context, UserManager<IdentityUser> userManager) => _context = context ,  _userManager = userManager;
        public AttendanceController(PayrollDbContext context,UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Employee"))
            {
                var userId = _userManager.GetUserId(User);

                var myAttendance = await _context.Attendances
                    .Include(a => a.Employee)
                    .Where(a => a.Employee.IdentityUserId == userId)
                    .OrderByDescending(a => a.Date)
                    .ToListAsync();

                return View(myAttendance);
            }

            // Admin / HR
            var data = await _context.Attendances
                .Include(a => a.Employee)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return View(data);
        }
        [Authorize(Roles = "Admin,HR")]
        public IActionResult Mark()
        {
            var employees = _context.Employees.Where(e => e.IsActive).ToList();
            return View(employees);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Mark(List<Attendances> attendanceList)
        {
            foreach (var att in attendanceList)
            {
                var existing = _context.Attendances
                    .FirstOrDefault(a => a.EmployeeId == att.EmployeeId && a.Date == att.Date);

                if (existing != null)
                    _context.Attendances.Remove(existing);

                // ✅ FIX: Calculate Working Hours using TimeSpan
                if (att.InTime.HasValue && att.OutTime.HasValue)
                {
                    att.WorkingHours = (att.OutTime.Value - att.InTime.Value).TotalHours;
                }
                else
                {
                    att.WorkingHours = 0;
                }

                _context.Attendances.Add(att);
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        //monthly payslip
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> MonthlySummary(int month, int year)
        {
            var userId = _userManager.GetUserId(User);

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.IdentityUserId == userId);

            var data = await _context.Attendances
                .Where(a => a.EmployeeId == employee.EmployeeId &&
                            a.Date.Month == month &&
                            a.Date.Year == year)
                .ToListAsync();

            ViewBag.Present = data.Count(a => a.Status == AttendanceStatus.Present);
            ViewBag.Absent = data.Count(a => a.Status == AttendanceStatus.Absent);
            ViewBag.Leave = data.Count(a => a.Status == AttendanceStatus.Leave);

            return View(data);
        }
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> EmployeeAttendance(int employeeId)
        {
            var data = await _context.Attendances
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return PartialView("_EmployeeAttendancePartial", data);
        }


    }
}
