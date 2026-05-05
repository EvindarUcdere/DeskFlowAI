using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskFlowAI.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentTextExtraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtractedTextPreview",
                table: "ProjectDocuments",
                type: "nvarchar(1600)",
                maxLength: 1600,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TextExtractedAt",
                table: "ProjectDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextExtractionStatus",
                table: "ProjectDocuments",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Not Extracted");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_TextExtractionStatus",
                table: "ProjectDocuments",
                column: "TextExtractionStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectDocuments_TextExtractionStatus",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "ExtractedTextPreview",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "TextExtractedAt",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "TextExtractionStatus",
                table: "ProjectDocuments");
        }
    }
}
