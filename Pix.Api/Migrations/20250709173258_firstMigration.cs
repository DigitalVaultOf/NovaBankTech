using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pix.Api.Migrations
{
    /// <inheritdoc />
    public partial class firstMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pix",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PixKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Bank = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pix", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionsTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Going = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Coming = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionsTable", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pix_PixKey",
                table: "Pix",
                column: "PixKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pix");

            migrationBuilder.DropTable(
                name: "TransactionsTable");
        }
    }
}
