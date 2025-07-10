using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pix.Api.Migrations
{
    /// <inheritdoc />
    public partial class correçãoDoMeuErro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AcconuntNumber",
                table: "Pix",
                newName: "AccountNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AccountNumber",
                table: "Pix",
                newName: "AcconuntNumber");
        }
    }
}
