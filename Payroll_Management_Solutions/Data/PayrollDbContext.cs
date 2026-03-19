using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Payroll_Management_Solutions.Models;

namespace Payroll_Management_Solutions.Data
{
    public class PayrollDbContext : IdentityDbContext<IdentityUser>
    {
        public PayrollDbContext(DbContextOptions<PayrollDbContext> options) : base(options) { }

        public DbSet<Employees> Employees { get; set; }
        public DbSet<Attendances> Attendances { get; set; }
        public DbSet<Payrolls> Payrolls { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<EmployeeNotifications> EmployeeNotifications { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ONLY schema configuration here
            builder.Entity<Employees>()
                   .Property(e => e.BasicSalary)
                   .HasPrecision(18, 2);

            builder.Entity<Payrolls>()
                   .Property(p => p.GrossSalary)
                   .HasPrecision(18, 2);

            builder.Entity<Payrolls>()
                   .Property(p => p.NetSalary)
                   .HasPrecision(18, 2);

            builder.Entity<Payrolls>()
                   .Property(p => p.Deductions)
                   .HasPrecision(18, 2);
            builder.Entity<EmployeeNotifications>()
                    .HasOne(en => en.Notification)
                    .WithMany()
                    .HasForeignKey(en => en.NotificationId)
                    .OnDelete(DeleteBehavior.Restrict); // 🔥 FIX
            builder.Entity<EmployeeNotifications>()
                .HasOne(en => en.Employee)
                .WithMany()
                .HasForeignKey(en => en.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

        }



        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    base.OnModelCreating(builder);

        //    // Optional: Seed roles
        //    builder.Entity<IdentityRole>().HasData(
        //        new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" },
        //        new IdentityRole { Name = "HR", NormalizedName = "HR" },
        //        new IdentityRole { Name = "Employee", NormalizedName = "EMPLOYEE" }
        //    );
        //}

    }
}
