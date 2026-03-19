using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payroll_Management_Solutions.Data;
using Payroll_Management_Solutions.Models;

namespace Payroll_Management_Solutions.Controllers
{
    [Authorize]
    public class LeaveController : Controller
    {
        private readonly PayrollDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LeaveController(PayrollDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ========== EMPLOYEE ==========

        [Authorize(Roles = "Employee")]
        public IActionResult Request()
        {
            var model = new LeaveRequest
            {
                FromDate = DateTime.Today,
                ToDate = DateTime.Today
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Request(LeaveRequest model)
        {
            var userId = _userManager.GetUserId(User);
            var employee = await _context.Employees
                                         .FirstOrDefaultAsync(e => e.IdentityUserId == userId);
            if (employee == null)
                return NotFound("Employee record not found for current user.");

            // These fields are not posted from the form; avoid validation errors for them.
            ModelState.Remove(nameof(model.EmployeeId));
            ModelState.Remove(nameof(model.Employee));
            ModelState.Remove(nameof(model.Status));
            ModelState.Remove(nameof(model.RequestedOn));
            ModelState.Remove(nameof(model.ReviewedOn));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = new LeaveRequest
            {
                EmployeeId = employee.EmployeeId,
                FromDate = model.FromDate.Date,
                ToDate = model.ToDate.Date,
                Reason = model.Reason,
                Status = LeaveStatus.Pending,
                RequestedOn = DateTime.Now
            };

            _context.LeaveRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave request submitted for approval.";
            return RedirectToAction(nameof(MyRequests));
        }

        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> MyRequests()
        {
            var userId = _userManager.GetUserId(User);
            var employee = await _context.Employees
                                         .FirstOrDefaultAsync(e => e.IdentityUserId == userId);
            if (employee == null)
                return NotFound("Employee record not found for current user.");

            var requests = await _context.LeaveRequests
                                         .Where(r => r.EmployeeId == employee.EmployeeId)
                                         .OrderByDescending(r => r.RequestedOn)
                                         .ToListAsync();

            return View(requests);
        }

        // ========== HR ==========

        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Index()
        {
            var requests = await _context.LeaveRequests
                                         .Include(r => r.Employee)
                                         .OrderBy(r => r.Status)
                                         .ThenByDescending(r => r.RequestedOn)
                                         .ToListAsync();
            return View(requests);
        }

        [HttpPost]
        [Authorize(Roles = "HR")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _context.LeaveRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = LeaveStatus.Approved;
            request.ReviewedOn = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave request approved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "HR")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _context.LeaveRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = LeaveStatus.Rejected;
            request.ReviewedOn = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave request rejected.";
            return RedirectToAction(nameof(Index));
        }
    }
}

