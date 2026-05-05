using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskFlowAI.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentFileCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileCheckMessage",
                table: "ProjectDocuments",
                type: "nvarchar(800)",
                maxLength: 800,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileCheckStatus",
                table: "ProjectDocuments",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Not Checked");

            migrationBuilder.AddColumn<DateTime>(
                name: "FileCheckedAt",
                table: "ProjectDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_FileCheckStatus",
                table: "ProjectDocuments",
                column: "FileCheckStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectDocuments_FileCheckStatus",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "FileCheckMessage",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "FileCheckStatus",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "FileCheckedAt",
                table: "ProjectDocuments");
        }
    }
}
