using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dZENcode_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddFilesInComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Document_FileName",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Document_Path",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Image_FileName",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Image_Path",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Document_FileName",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Document_Path",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Image_FileName",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Image_Path",
                table: "Comments");
        }
    }
}
