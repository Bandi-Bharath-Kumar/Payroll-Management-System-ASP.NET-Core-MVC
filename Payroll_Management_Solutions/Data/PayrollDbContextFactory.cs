using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Payroll_Management_Solutions.Data
{
    public class PayrollDbContextFactory
        : IDesignTimeDbContextFactory<PayrollDbContext>
    {
        public PayrollDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PayrollDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=.;Database=PayrollDB;Trusted_Connection=True;TrustServerCertificate=True");

            return new PayrollDbContext(optionsBuilder.Options);
        }
    }
}
