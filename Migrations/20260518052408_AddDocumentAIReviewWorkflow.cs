using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskFlowAI.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentAIReviewWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AIReviewStatus",
                table: "ProjectDocuments",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Not Ready");

            migrationBuilder.AddColumn<DateTime>(
                name: "AIReviewedAt",
                table: "ProjectDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AIReviewedByEmail",
                table: "ProjectDocuments",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE ProjectDocuments SET AIReviewStatus = CASE WHEN AIAnalysisStatus = 'Analyzed' THEN 'Ready' ELSE 'Not Ready' END");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_AIReviewStatus",
                table: "ProjectDocuments",
                column: "AIReviewStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectDocuments_AIReviewStatus",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "AIReviewStatus",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "AIReviewedAt",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "AIReviewedByEmail",
                table: "ProjectDocuments");
        }
    }
}
