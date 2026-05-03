using System;
using DeskFlowAI.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskFlowAI.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DeskFlowDbContext))]
    [Migration("20260503120000_AddEmployees")]
    public partial class AddEmployees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 140, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    RoleTitle = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AvailabilityStatus = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    LeaveStart = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LeaveEnd = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Skills = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    BackupEmployeeName = table.Column<string>(type: "TEXT", maxLength: 140, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
