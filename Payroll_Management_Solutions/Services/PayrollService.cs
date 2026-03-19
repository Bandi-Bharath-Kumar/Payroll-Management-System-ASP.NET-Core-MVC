using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Microsoft.EntityFrameworkCore;
using Payroll_Management_Solutions.Data;
using Payroll_Management_Solutions.Models;
using System.IO;


namespace Payroll_Management_Solutions.Services
{
    public class PayrollService
    {
        private readonly PayrollDbContext _context;

        public PayrollService(PayrollDbContext context)
        {
            _context = context;
        }
        // Auto-generate payroll for all active employees

        public async Task GenerateMonthlyPayroll(int month, int year)
        {
            // 1️⃣ Get all active employees
            var employees = await _context.Employees
                .Where(e => e.IsActive)
                .ToListAsync();

            // 2️⃣ Get already generated payroll employee IDs (IMPORTANT FIX)
            var existingEmployeeIds = await _context.Payrolls
                .Where(p => p.Month == month && p.Year == year)
                .Select(p => p.EmployeeId)
                .ToListAsync();

            foreach (var emp in employees)
            {
                // 3️⃣ Skip only employees who already have payroll
                if (existingEmployeeIds.Contains(emp.EmployeeId))
                    continue;

                int totalDays = DateTime.DaysInMonth(year, month);
                int presentDays = totalDays;

                decimal perDaySalary = emp.BasicSalary / totalDays;
                decimal grossSalary = perDaySalary * presentDays;

                var payroll = new Payrolls
                {
                    EmployeeId = emp.EmployeeId,
                    Month = month,
                    Year = year,
                    TotalWorkingDays = totalDays,
                    PresentDays = presentDays,
                    BasicSalary = emp.BasicSalary,
                    GrossSalary = grossSalary,
                    Deductions = 0,
                    NetSalary = grossSalary
                };

                _context.Payrolls.Add(payroll);
            }

            // 4️⃣ Save once after loop
            await _context.SaveChangesAsync();
        }

        public byte[] GeneratePayslip(Payrolls p)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            var titleFont = new XFont("Arial", 18, XFontStyle.Bold);
            var textFont = new XFont("Arial", 12, XFontStyle.Bold);
            var brush = XBrushes.Black;

            // Title
            gfx.DrawString(
                "PAYSLIP",
                titleFont,
                brush,
                new XRect(0, 20, page.Width, 30),
                XStringFormats.Center
            );

            int y = 80;

            gfx.DrawString($"Employee Name : {p.Employee.FullName}", textFont, brush, 40, y);
            y += 25;
            gfx.DrawString($"Month / Year  : {p.Month}/{p.Year}", textFont, brush, 40, y);
            y += 25;
            gfx.DrawString($"Basic Salary : {p.BasicSalary}", textFont, brush, 40, y);
            y += 25;
            gfx.DrawString($"Net Salary   : {p.NetSalary}", textFont, brush, 40, y);

            using var ms = new MemoryStream();
            document.Save(ms);
            return ms.ToArray();
        }





    }
}
