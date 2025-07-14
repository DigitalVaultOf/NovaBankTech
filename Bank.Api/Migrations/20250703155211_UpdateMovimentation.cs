using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace User.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMovimentation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "value",
                table: "Moviments",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "Moviments",
                newName: "DateTimeMoviment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateTimeMoviment",
                table: "Moviments",
                newName: "DateTime");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Moviments",
                newName: "value");
        }
    }
}
