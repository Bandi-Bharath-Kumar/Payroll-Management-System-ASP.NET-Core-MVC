using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payroll_Management_Solutions.Data;
using Payroll_Management_Solutions.Models;

namespace Payroll_Management_Solutions.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly PayrollDbContext _context;

        public EventsController(PayrollDbContext context)
        {
            _context = context;
        }

        // All authenticated users (Admin, HR, Employee) can view events
        public async Task<IActionResult> Index()
        {
            var eventsQuery = _context.Notifications
                                      .Where(n => n.NotificationType == "Event")
                                      .OrderByDescending(n => n.CreatedDate);

            var events = await eventsQuery.ToListAsync();
            return View(events);
        }

        // Only Admin/HR can create events
        [Authorize(Roles = "Admin,HR")]
        public IActionResult Create()
        {
            var model = new Notifications
            {
                NotificationType = "Event",
                CreatedDate = DateTime.Now,
                IsForAll = true
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Notifications model)
        {
            // These fields are not posted by the form; set them before validation to avoid implicit-required failures.
            model.NotificationType = "Event";
            model.IsForAll = true;
            ModelState.Remove(nameof(model.NotificationType));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.CreatedDate == default)
            {
                model.CreatedDate = DateTime.Now;
            }

            // CreatedBy is required; if you want per-employee ownership later we can wire it,
            // for now store 0 to indicate a system/admin event.
            if (model.CreatedBy == 0)
            {
                model.CreatedBy = 0;
            }

            _context.Notifications.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Event created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Only Admin/HR can edit events
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Edit(int id)
        {
            var evt = await _context.Notifications
                                    .FirstOrDefaultAsync(n => n.NotificationId == id &&
                                                              n.NotificationType == "Event");
            if (evt == null) return NotFound();

            return View(evt);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Notifications model)
        {
            if (id != model.NotificationId) return NotFound();

            model.NotificationType = "Event";
            model.IsForAll = true;
            ModelState.Remove(nameof(model.NotificationType));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existing = await _context.Notifications
                                         .FirstOrDefaultAsync(n => n.NotificationId == id &&
                                                                   n.NotificationType == "Event");
            if (existing == null) return NotFound();

            existing.Title = model.Title;
            existing.Message = model.Message;
            existing.CreatedDate = model.CreatedDate == default ? existing.CreatedDate : model.CreatedDate;
            existing.IsForAll = true;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Event updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Only Admin/HR can delete events
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Delete(int id)
        {
            var evt = await _context.Notifications
                                    .FirstOrDefaultAsync(n => n.NotificationId == id &&
                                                              n.NotificationType == "Event");
            if (evt == null) return NotFound();

            return View(evt);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var evt = await _context.Notifications
                                    .FirstOrDefaultAsync(n => n.NotificationId == id &&
                                                              n.NotificationType == "Event");
            if (evt == null) return NotFound();

            _context.Notifications.Remove(evt);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Event deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}

