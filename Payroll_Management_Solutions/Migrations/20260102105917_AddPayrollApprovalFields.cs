using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll_Management_Solutions.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedOn",
                table: "Payrolls",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedOn",
                table: "Payrolls");
        }
    }
}
