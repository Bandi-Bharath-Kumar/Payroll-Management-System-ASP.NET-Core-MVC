using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payroll_Management_Solutions.Data;
using Payroll_Management_Solutions.Models;
using Payroll_Management_Solutions.Models.ViewModels;

namespace Payroll_Management_Solutions.Controllers
{
    [Authorize] // All actions require authentication
    public class DashboardController : Controller
    {
        private readonly PayrollDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(PayrollDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ================= ADMIN =================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            // Count all employees (active and inactive)
            var totalEmployees = await _context.Employees.CountAsync();
            var pendingApprovals = await _context.Payrolls.CountAsync(p => !p.IsApproved);
            var hrCount = await _context.Employees
                                        .CountAsync(e => e.Role == "HR");

            var today = DateTime.Today;
            var todayEvents = await _context.Notifications
                                            .Where(n => n.NotificationType == "Event" &&
                                                        n.CreatedDate.Date >= today)
                                            .OrderBy(n => n.CreatedDate)
                                            .ToListAsync();

            var vm = new AdminDashboardVM
            {
                TotalEmployees = totalEmployees,
                PendingApprovals = pendingApprovals,
                HrMembers = hrCount,
                TodayEvents = todayEvents
            };

            return View(vm);
        }

        // ================= HR =================
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> HrDashboard()
        {
            var totalEmployees = await _context.Employees.CountAsync();
            var activeEmployees = await _context.Employees.CountAsync(e => e.IsActive);
            var employees = await _context.Employees
                                          .OrderBy(e => e.FullName)
                                          .ToListAsync();

            var vm = new HrDashboardVM
            {
                TotalEmployees = totalEmployees,
                ActiveEmployees = activeEmployees,
                Employees = employees
            };

            return View(vm);
        }

        // ================= EMPLOYEE =================
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> EmployeeDashboard()
        {
            var userId = _userManager.GetUserId(User);

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.IdentityUserId == userId);

            if (employee == null)
                return NotFound();

            // ================= ATTENDANCE =================
            var attendanceData = await _context.Attendances
                .Where(a => a.EmployeeId == employee.EmployeeId &&
                            a.Date.Month == DateTime.Now.Month &&
                            a.Date.Year == DateTime.Now.Year)
                .GroupBy(a => a.Status)
                .Select(g => new
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            // ================= PAYROLL =================
            var payrollData = await _context.Payrolls
                .Where(p => p.EmployeeId == employee.EmployeeId && p.IsApproved)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .Take(6)
                .OrderBy(p => p.Year)
                .ThenBy(p => p.Month)
                .Select(p => new
                {
                    Month = p.Month + "/" + p.Year,
                    Amount = p.NetSalary
                })
                .ToListAsync();

            var dashboardVM = new EmployeeDashboardVM
            {
                AttendanceLabels = attendanceData.Select(a => a.Status).ToList(),
                AttendanceCounts = attendanceData.Select(a => a.Count).ToList(),
                PayrollMonths = payrollData.Select(p => p.Month).ToList(),
                PayrollAmounts = payrollData.Select(p => p.Amount).ToList()
            };

            var pageVM = new EmployeeDashboardPageVM
            {
                Employee = employee,
                Dashboard = dashboardVM
            };

            return View(pageVM);
        }


        // ================= REDIRECT =================
        public IActionResult RedirectToDashboard()
        {
            if (User.IsInRole("Admin")) return RedirectToAction("AdminDashboard");
            if (User.IsInRole("HR")) return RedirectToAction("HRDashboard");
            if (User.IsInRole("Employee")) return RedirectToAction("EmployeeDashboard");

            return RedirectToAction("Login", "Account");
        }
    }
}
