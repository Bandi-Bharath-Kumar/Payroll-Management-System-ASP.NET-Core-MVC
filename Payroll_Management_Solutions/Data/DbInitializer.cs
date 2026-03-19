using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Payroll_Management_Solutions.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndUsers(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roles = { "Admin", "HR", "Employee" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Create default Admin
            if (await userManager.FindByEmailAsync("admin@company.com") == null)
            {
                var admin = new IdentityUser { UserName = "admin@company.com", Email = "admin@company.com", EmailConfirmed = true };
                await userManager.CreateAsync(admin, "Admin@123"); // default password
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Create default HR
            if (await userManager.FindByEmailAsync("hr@company.com") == null)
            {
                var hr = new IdentityUser { UserName = "hr@company.com", Email = "hr@company.com", EmailConfirmed = true };
                await userManager.CreateAsync(hr, "HR@123");
                await userManager.AddToRoleAsync(hr, "HR");
            }

            // Create default Employee
            if (await userManager.FindByEmailAsync("employee@company.com") == null)
            {
                var emp = new IdentityUser { UserName = "employee@company.com", Email = "employee@company.com", EmailConfirmed = true };
                await userManager.CreateAsync(emp, "Emp@123");
                await userManager.AddToRoleAsync(emp, "Employee");
            }
        }
    }
}
