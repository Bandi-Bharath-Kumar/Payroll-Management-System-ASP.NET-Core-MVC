using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll_Management_Solutions.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollApprovalColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "Payrolls",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Payrolls",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Payrolls");
        }
    }
}
