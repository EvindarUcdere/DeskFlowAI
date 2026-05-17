using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskFlowAI.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskDependency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlockedBy",
                table: "Tasks",
                type: "nvarchar(220)",
                maxLength: 220,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_BlockedBy",
                table: "Tasks",
                column: "BlockedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_BlockedBy",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "BlockedBy",
                table: "Tasks");
        }
    }
}
