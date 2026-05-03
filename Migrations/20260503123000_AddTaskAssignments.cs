using DeskFlowAI.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskFlowAI.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DeskFlowDbContext))]
    [Migration("20260503123000_AddTaskAssignments")]
    public partial class AddTaskAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedEmployeeId",
                table: "Tasks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedEmployeeId",
                table: "Tasks",
                column: "AssignedEmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_AssignedEmployeeId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "AssignedEmployeeId",
                table: "Tasks");
        }
    }
}
