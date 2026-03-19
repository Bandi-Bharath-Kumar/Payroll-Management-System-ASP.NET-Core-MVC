using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payroll_Management_Solutions.Data;
using Payroll_Management_Solutions.Models;

[Authorize]
public class NotificationController : Controller
{
    private readonly NotificationRepository _repo;


    public NotificationController(NotificationRepository repo)
    {
        _repo = repo;
      
    }
   

    // ================= HR CREATES NOTIFICATION =================
    [HttpPost]
    [Authorize(Roles = "HR")]
    public IActionResult CreateNotification(Notifications model, List<int> employeeIds)
    {
        if (employeeIds == null || !employeeIds.Any())
        {
            TempData["Error"] = "Please select at least one employee.";
            return RedirectToAction("Index", "Dashboard");
        }

        model.CreatedDate = DateTime.Now;
        model.CreatedBy = GetLoggedEmployeeId();

        int notificationId = _repo.InsertNotification(model);

        foreach (var empId in employeeIds)
        {
            _repo.AssignNotification(notificationId, empId);
        }

        TempData["Success"] = "Notification sent successfully!";
        return RedirectToAction("Index", "Dashboard");
    }

    // ================= EMPLOYEE FULL NOTIFICATION PAGE =================
    public IActionResult MyNotifications()
    {
        int employeeId = GetLoggedEmployeeId();

        var notifications = _repo.GetEmployeeNotifications(employeeId);
        return View(notifications);
    }

    // ================= NOTIFICATION BELL (TOP 5) =================
    public IActionResult NotificationBell()
    {
        int employeeId = GetLoggedEmployeeId();

        var notifications = _repo.GetEmployeeNotifications(employeeId)
                                 .Take(5)
                                 .ToList();

        ViewBag.UnreadCount = notifications.Count(n => !n.IsRead);

        return PartialView("_NotificationBell", notifications);
    }

    // ================= COMMON METHOD =================
    private int GetLoggedEmployeeId()
    {
        var claim = User.FindFirst("EmployeeId");
        if (claim != null && int.TryParse(claim.Value, out int empId))
            return empId;

        var identityUserId =
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(identityUserId))
            throw new UnauthorizedAccessException("User not logged in.");

        var employeeId = _repo.GetEmployeeIdByIdentityUserId(identityUserId);

        if (employeeId.HasValue)
            return employeeId.Value;

        var userEmail = User.Identity?.Name ?? "unknown@system.local";

        var employee = new Employees
        {
            Email = userEmail,
            FullName = userEmail,
            Role = "Employee",
            IsActive = true,
            IdentityUserId = identityUserId
        };

        _repo.CreateEmployee(employee);
        return employee.EmployeeId;
    }





}
