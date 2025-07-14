using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pix.Api.Migrations
{
    /// <inheritdoc />
    public partial class outraAttDeBanco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Pix",
                newName: "IdPix");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdPix",
                table: "Pix",
                newName: "Id");
        }
    }
}
