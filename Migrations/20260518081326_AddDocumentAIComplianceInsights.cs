using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskFlowAI.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentAIComplianceInsights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AIComplianceStatus",
                table: "ProjectDocuments",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "Passed");

            migrationBuilder.AddColumn<string>(
                name: "AIPolicyViolations",
                table: "ProjectDocuments",
                type: "nvarchar(1200)",
                maxLength: 1200,
                nullable: false,
                defaultValue: "No policy violations detected");

            migrationBuilder.AddColumn<int>(
                name: "AIRiskScore",
                table: "ProjectDocuments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_AIComplianceStatus",
                table: "ProjectDocuments",
                column: "AIComplianceStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_AIRiskScore",
                table: "ProjectDocuments",
                column: "AIRiskScore");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectDocuments_AIComplianceStatus",
                table: "ProjectDocuments");

            migrationBuilder.DropIndex(
                name: "IX_ProjectDocuments_AIRiskScore",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "AIComplianceStatus",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "AIPolicyViolations",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "AIRiskScore",
                table: "ProjectDocuments");
        }
    }
}
