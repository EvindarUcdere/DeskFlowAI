using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskFlowAI.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentAIAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AIAnalysisStatus",
                table: "ProjectDocuments",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Not Analyzed");

            migrationBuilder.AddColumn<string>(
                name: "AIRiskNotes",
                table: "ProjectDocuments",
                type: "nvarchar(1200)",
                maxLength: 1200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AISummary",
                table: "ProjectDocuments",
                type: "nvarchar(1200)",
                maxLength: 1200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "AnalyzedAt",
                table: "ProjectDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_AIAnalysisStatus",
                table: "ProjectDocuments",
                column: "AIAnalysisStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectDocuments_AIAnalysisStatus",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "AIAnalysisStatus",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "AIRiskNotes",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "AISummary",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "AnalyzedAt",
                table: "ProjectDocuments");
        }
    }
}
