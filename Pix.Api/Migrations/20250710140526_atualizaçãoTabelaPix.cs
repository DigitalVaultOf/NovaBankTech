using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pix.Api.Migrations
{
    /// <inheritdoc />
    public partial class atualizaçãoTabelaPix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Pix",
                newName: "NameUser");

            migrationBuilder.AddColumn<string>(
                name: "AcconuntNumber",
                table: "Pix",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcconuntNumber",
                table: "Pix");

            migrationBuilder.RenameColumn(
                name: "NameUser",
                table: "Pix",
                newName: "Name");
        }
    }
}
