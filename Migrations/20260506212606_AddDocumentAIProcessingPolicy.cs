using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskFlowAI.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentAIProcessingPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AIProcessingPolicy",
                table: "ProjectDocuments",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "Internal Only");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_AIProcessingPolicy",
                table: "ProjectDocuments",
                column: "AIProcessingPolicy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectDocuments_AIProcessingPolicy",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "AIProcessingPolicy",
                table: "ProjectDocuments");
        }
    }
}
