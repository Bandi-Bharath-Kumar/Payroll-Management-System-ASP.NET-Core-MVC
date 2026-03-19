using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll_Management_Solutions.Migrations
{
    /// <inheritdoc />
    public partial class FixEmployeeNotificationRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeNotifications_Notifications_NotificationId",
                table: "EmployeeNotifications");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "EmployeeNotifications",
                newName: "EmployeeNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeNotifications_EmployeeId",
                table: "EmployeeNotifications",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeNotifications_Employees_EmployeeId",
                table: "EmployeeNotifications",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeNotifications_Notifications_NotificationId",
                table: "EmployeeNotifications",
                column: "NotificationId",
                principalTable: "Notifications",
                principalColumn: "NotificationId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeNotifications_Employees_EmployeeId",
                table: "EmployeeNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeNotifications_Notifications_NotificationId",
                table: "EmployeeNotifications");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeNotifications_EmployeeId",
                table: "EmployeeNotifications");

            migrationBuilder.RenameColumn(
                name: "EmployeeNotificationId",
                table: "EmployeeNotifications",
                newName: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeNotifications_Notifications_NotificationId",
                table: "EmployeeNotifications",
                column: "NotificationId",
                principalTable: "Notifications",
                principalColumn: "NotificationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
