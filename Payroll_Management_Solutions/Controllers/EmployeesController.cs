using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X509;
using Payroll_Management_Solutions.Data;
using Payroll_Management_Solutions.Models;
using Payroll_Management_Solutions.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll_Management_Solutions.Controllers
{
    //[Authorize(Roles = "Admin,HR")]
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly PayrollDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EmployeesController(PayrollDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> MyProfile()
        {
            var userId = _userManager.GetUserId(User);

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.IdentityUserId == userId);

            if (employee == null)
                return NotFound("Employee profile not found");

            // 🔥 If request is AJAX (hover), return partial view
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_MyProfileHover", employee);
            }

            // Normal full page view
            return View(employee);
        }


        [Authorize(Roles = "Admin,HR")]
        public IActionResult Index()
        {
            return View(_context.Employees.ToList());
        }

        // ================= CREATE =================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = new List<string> { "Admin", "HR", "Employee" };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employees employee)
        {
            //LOG validation errors BEFORE returning view
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                //IMPORTANT: Reload ViewBag data
                ViewBag.Roles = new List<string> { "Admin", "HR", "Employee" };

                return View(employee);
            }

            employee.IsActive = true;

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Employee added successfully";
            return RedirectToAction(nameof(Index));
        }




        // ================= EDIT =================

        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public IActionResult Edit(int id)
        {
            var emp = _context.Employees.Find(id);
            if (emp == null) return NotFound();

            ViewBag.Roles = new[] { "Admin", "HR", "Employee" };
            return View(emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Edit(int id, Employees employee)
        {
            if (id != employee.EmployeeId)
                return NotFound();

            var existing = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (existing == null)
                return NotFound();

            // HR cannot activate/deactivate Admin employees (or edit them)
            if (User.IsInRole("HR") && string.Equals(existing.Role, "Admin", System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "You are not allowed to modify an Admin employee.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new[] { "Admin", "HR", "Employee" };
                return View(employee);
            }

            _context.Update(employee);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Employee updated successfully";
            return RedirectToAction(nameof(Index));
        }

        // ================= DELETE =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            if (emp.IdentityUserId == currentUserId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(emp.IdentityUserId);
            if (user != null)
                await _userManager.DeleteAsync(user);

            _context.Employees.Remove(emp);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Employee deleted successfully.";
            return RedirectToAction(nameof(Index));
        }



        /// ==============MY Profile

        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> MyPayrolls()
        {
            var userId = _userManager.GetUserId(User);

            var payrolls = await _context.Payrolls
                .Include(p => p.Employee)
                .Where(p =>
                    p.IsApproved &&
                    p.Employee.IdentityUserId == userId)
                .ToListAsync();

            return View(payrolls);
        }

        // login for all employees
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> CreateLogin(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            // Already has login
            if (!string.IsNullOrEmpty(emp.IdentityUserId))
            {
                TempData["Error"] = "Login already exists for this employee.";
                return RedirectToAction(nameof(Index));
            }

            var user = new IdentityUser
            {
                UserName = emp.Email,
                Email = emp.Email,
                EmailConfirmed = true
            };

            string defaultPassword = "Emp@123"; // 🔥 TEMP PASSWORD

            var result = await _userManager.CreateAsync(user, defaultPassword);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.Errors.First().Description;
                return RedirectToAction(nameof(Index));
            }

            var roleName = string.IsNullOrWhiteSpace(emp.Role) ? "Employee" : emp.Role;
            await _userManager.AddToRoleAsync(user, roleName);

            emp.IdentityUserId = user.Id;
            emp.NeedPasswordReset = true;
            _context.SaveChanges();

            TempData["Success"] = $"Login created. Default password: {defaultPassword}";
            return RedirectToAction(nameof(Index));
        }

        // Simple redirect for legacy /Employees/MyAttendance links
        [Authorize(Roles = "Employee")]
        public IActionResult MyAttendance()
        {
            return RedirectToAction("Index", "Attendance");
        }


    }
}
