using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dZENcode_backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNamePropsComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Image_FileName",
                table: "Comments",
                newName: "Image_Name");

            migrationBuilder.RenameColumn(
                name: "Document_FileName",
                table: "Comments",
                newName: "Document_Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Image_Name",
                table: "Comments",
                newName: "Image_FileName");

            migrationBuilder.RenameColumn(
                name: "Document_Name",
                table: "Comments",
                newName: "Document_FileName");
        }
    }
}
