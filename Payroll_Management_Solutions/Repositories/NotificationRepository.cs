using Microsoft.EntityFrameworkCore;
using Payroll_Management_Solutions.Data;
using Payroll_Management_Solutions.Models;
using Payroll_Management_Solutions.Models.ViewModels;

public class NotificationRepository
{
    private readonly PayrollDbContext _context;

    public NotificationRepository(PayrollDbContext context)
    {
        _context = context;
    }

    public int InsertNotification(Notifications model)
    {
        _context.Notifications.Add(model);
        _context.SaveChanges();
        return model.NotificationId;
    }

    public void AssignNotification(int notificationId, int employeeId)
    {
        var empNotification = new EmployeeNotifications
        {
            NotificationId = notificationId,
            EmployeeId = employeeId,
            IsRead = false
        };

        _context.EmployeeNotifications.Add(empNotification);
        _context.SaveChanges();
    }

    public List<EmployeeNotificationVM> GetEmployeeNotifications(int employeeId)
    {
        return _context.EmployeeNotifications
            .Include(en => en.Notification) // ✅ important
            .Where(en => en.EmployeeId == employeeId)
            .OrderByDescending(en => en.Notification.CreatedDate)
            .Select(en => new EmployeeNotificationVM
            {
                Id = en.EmployeeNotificationId,
                Title = en.Notification.Title,
                Message = en.Notification.Message,
                NotificationType = en.Notification.NotificationType,
                IsRead = en.IsRead,
                CreatedDate = en.Notification.CreatedDate
            })
            .ToList();
    }

    public int? GetEmployeeIdByIdentityUserId(string identityUserId)
    {
        return _context.Employees
            .Where(e => e.IdentityUserId == identityUserId)
            .Select(e => (int?)e.EmployeeId)
            .FirstOrDefault();
    }

    public Employees CreateEmployee(Employees employee)
    {
        _context.Employees.Add(employee);
        _context.SaveChanges();
        return employee;
    }
}
