using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartBaby.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPreviewImageToBabyAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "PreviewImage",
                table: "BabyAnalyses",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviewImageContentType",
                table: "BabyAnalyses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviewImage",
                table: "BabyAnalyses");

            migrationBuilder.DropColumn(
                name: "PreviewImageContentType",
                table: "BabyAnalyses");
        }
    }
}
