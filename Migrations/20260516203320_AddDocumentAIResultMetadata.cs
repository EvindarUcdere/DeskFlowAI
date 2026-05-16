using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskFlowAI.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentAIResultMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AIConfidenceScore",
                table: "ProjectDocuments",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AIDetectedIssues",
                table: "ProjectDocuments",
                type: "nvarchar(1200)",
                maxLength: 1200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AIProviderName",
                table: "ProjectDocuments",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AIRecommendations",
                table: "ProjectDocuments",
                type: "nvarchar(1200)",
                maxLength: 1200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AIRiskLevel",
                table: "ProjectDocuments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "AIUsedFallback",
                table: "ProjectDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_AIProviderName",
                table: "ProjectDocuments",
                column: "AIProviderName");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_AIRiskLevel",
                table: "ProjectDocuments",
                column: "AIRiskLevel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectDocuments_AIProviderName",
                table: "ProjectDocuments");

            migrationBuilder.DropIndex(
                name: "IX_ProjectDocuments_AIRiskLevel",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "AIConfidenceScore",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "AIDetectedIssues",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "AIProviderName",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "AIRecommendations",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "AIRiskLevel",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "AIUsedFallback",
                table: "ProjectDocuments");
        }
    }
}
