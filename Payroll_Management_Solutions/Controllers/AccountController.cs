using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payroll_Management_Solutions.Models;
using Payroll_Management_Solutions.Models.ViewModels;
using System.Threading.Tasks;
using Payroll_Management_Solutions.Data;
using System.Collections.Generic; // ✅ REQUIRED
using System.Security.Claims;

namespace Payroll_Management_Solutions.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly PayrollDbContext _context;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            PayrollDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);

                if (model.Role == "Employee" || model.Role == "HR" || model.Role == "Admin")
                {
                    var employee = new Employees
                    {
                        FullName = model.FullName,
                        Email = model.Email,
                        Role = model.Role,
                        IsActive = true,
                        IdentityUserId = user.Id
                    };

                    _context.Employees.Add(employee);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName,
                model.Password,
                false,
                false
            );

            if (result.Succeeded)
            {
                // 🔥 EMPLOYEE CLAIM
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.IdentityUserId == user.Id);

                if (employee != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim("EmployeeId", employee.EmployeeId.ToString()),
                        new Claim("FullName", employee.FullName ?? user.Email ?? user.UserName ?? string.Empty)
                    };

                    await _userManager.AddClaimsAsync(user, claims);
                    await _signInManager.RefreshSignInAsync(user);
                }

                // Force password reset on first login if required
                if (employee != null && employee.NeedPasswordReset)
                {
                    return RedirectToAction(nameof(ForcePasswordReset));
                }

                // ✅ ROLE-BASED REDIRECT (FIXED)
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("AdminDashboard", "Dashboard");

                if (await _userManager.IsInRoleAsync(user, "HR"))
                    return RedirectToAction("HrDashboard", "Dashboard"); // ✅ FIXED

                if (await _userManager.IsInRoleAsync(user, "Employee"))
                    return RedirectToAction("EmployeeDashboard", "Dashboard");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize]
        public IActionResult ForcePasswordReset()
        {
            return View(new ForcePasswordResetViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForcePasswordReset(ForcePasswordResetViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Login");
            }

            var changeResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!changeResult.Succeeded)
            {
                foreach (var error in changeResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Mark employee as no longer needing reset
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.IdentityUserId == user.Id);

            if (employee != null)
            {
                employee.NeedPasswordReset = false;
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Password reset successfully. Please use your new password from now on.";
            return RedirectToAction("RedirectToDashboard", "Dashboard");
        }
    }
}
