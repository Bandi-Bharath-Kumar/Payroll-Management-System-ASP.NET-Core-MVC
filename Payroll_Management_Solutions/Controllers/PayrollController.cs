using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payroll_Management_Solutions.Data;
using Payroll_Management_Solutions.Models;
using Payroll_Management_Solutions.Services;

namespace Payroll_Management_Solutions.Controllers
{
    [Authorize]
    public class PayrollController : Controller
    {
        private readonly PayrollDbContext _context;
        private readonly PayrollService _payrollService;
        private readonly UserManager<IdentityUser> _userManager;

        public PayrollController(PayrollDbContext context, PayrollService payrollService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _payrollService = payrollService;
            _userManager = userManager;
        }

        // =========================
        // 1️⃣ ROLE-BASED PAYROLL VIEW
        // =========================
        [Authorize]
        public IActionResult Index()
        {
            IQueryable<Payrolls> payrolls = _context.Payrolls
                .Include(p => p.Employee)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month);

            if (User.IsInRole("Employee"))
            {
                var userId = _userManager.GetUserId(User);
                //var email = User.Identity.Name;

                payrolls = payrolls
                    .Where(p =>
                        p.IsApproved &&
                        //p.Employee.Email == email &&
                        p.Employee.IdentityUserId == userId);
            }
            // HR & Admin automatically see everything

            return View(payrolls.ToList());
        }


        // =========================
        // 2️⃣ HR – MANUAL CREATE
        // =========================
        [Authorize(Roles = "HR")]
        public IActionResult Create()
        {
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            ViewBag.Employees = _context.Employees
                .Where(e => e.IsActive)
                .Select(e => new
                {
                    e.EmployeeId,
                    e.FullName,
                    HasPayroll = _context.Payrolls.Any(p =>
                        p.EmployeeId == e.EmployeeId &&
                        p.Month == month &&
                        p.Year == year)
                })
                .ToList();

            return View();
        }
        [Authorize(Roles = "HR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Payrolls payroll)
        {
            foreach (var entry in ModelState)
            {
                if (entry.Value.Errors.Count > 0)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"MODEL ERROR → {entry.Key} : {error.ErrorMessage}");
                    }
                }
            }
            int month = payroll.Month;
            int year = payroll.Year;

            ViewBag.Employees = _context.Employees
                .Where(e => e.IsActive)
                .Select(e => new
                {
                    e.EmployeeId,
                    e.FullName,
                    HasPayroll = _context.Payrolls.Any(p =>
                        p.EmployeeId == e.EmployeeId &&
                        p.Month == month &&
                        p.Year == year)
                })
                .ToList();

            //if (payroll.EmployeeId == 0)
            //{
            //    ModelState.AddModelError("EmployeeId", "Please select an employee.");
            //}

            //if (payroll.TotalWorkingDays <= 0)
            //{
            //    ModelState.AddModelError("TotalWorkingDays", "Total Working Days must be greater than 0.");
            //}

            if (!ModelState.IsValid)
                return View(payroll);

            bool exists = _context.Payrolls.Any(p =>
                                p.EmployeeId == payroll.EmployeeId &&
                                p.Month == payroll.Month &&
                                p.Year == payroll.Year);

            if (exists)
            {
                ModelState.AddModelError("EmployeeId", "Payroll already generated for this employee for selected month.");
                return View(payroll);
            }

            var employee = _context.Employees.Find(payroll.EmployeeId);
            if (employee == null)
            {
                ModelState.AddModelError("", "Invalid employee selected.");
                return View(payroll);
            }

            payroll.BasicSalary = employee.BasicSalary;
            payroll.IsApproved = false;
            payroll.ApprovedBy = null;

            decimal perDaySalary = payroll.BasicSalary / payroll.TotalWorkingDays;
            payroll.GrossSalary = perDaySalary * payroll.PresentDays;
            payroll.NetSalary = payroll.GrossSalary - payroll.Deductions;

            _context.Payrolls.Add(payroll);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }



        // =========================
        // 3️⃣ HR – AUTO GENERATE
        // =========================
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Generate(int? month, int? year)
        {
            month ??= DateTime.Now.Month;
            year ??= DateTime.Now.Year;

            await _payrollService.GenerateMonthlyPayroll(month.Value, year.Value);

            TempData["Message"] = $"Payroll generated for {month}/{year}.";
            return RedirectToAction(nameof(Index));
        }


        // =========================
        // 4️⃣ ADMIN – APPROVE PAYROLL
        // =========================
        [Authorize(Roles = "Admin")]
        public IActionResult Approve(int id)
        {
            var payroll = _context.Payrolls.FirstOrDefault(p => p.PayrollId == id);

            if (payroll == null || payroll.IsApproved)
                return NotFound();

            payroll.IsApproved = true;
            payroll.ApprovedBy = User.Identity.Name;
            payroll.ApprovedOn = DateTime.Now; // ✅ Recommended

            _context.SaveChanges();

            TempData["Message"] = "Payroll approved successfully.";
            return RedirectToAction(nameof(Index));
        }


        // =========================
        // 5️⃣ EMPLOYEE – VIEW ONLY APPROVED
        // =========================
        [Authorize(Roles = "Employee")]
        public IActionResult MyPayrolls()
        {
            var email = User.Identity.Name;

            var payrolls = _context.Payrolls
                .Include(p => p.Employee)
                .Where(p => p.Employee.Email == email && p.IsApproved)
                .ToList();

            return View(payrolls);
        }


        // =========================
        // 6️⃣ DOWNLOAD PAYSLIP
        // =========================
        [Authorize]
        public IActionResult DownloadPayslip(int id)
        {
            var payroll = _context.Payrolls
                .Include(p => p.Employee)
                .FirstOrDefault(p => p.PayrollId == id && p.IsApproved);

            if (payroll == null)
                return Unauthorized();

            // Employee can only download own payslip
            //if (User.IsInRole("Employee") &&
            //    payroll.Employee.Email != User.Identity.Name)
            //{
            //    return Unauthorized();
            //}
            if (User.IsInRole("Employee"))
            {
                var userId = _userManager.GetUserId(User);

                if (payroll.Employee.IdentityUserId != userId)
                    return Unauthorized();
            }
            var pdf = _payrollService.GeneratePayslip(payroll);
            return File(pdf, "application/pdf", "Payslip.pdf");
        }

    }
}
