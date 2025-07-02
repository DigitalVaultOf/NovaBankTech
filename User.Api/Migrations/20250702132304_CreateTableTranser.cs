using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace User.Api.Migrations
{
    /// <inheritdoc />
    public partial class CreateTableTranser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNumberFrom = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccountNumberTo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transfers_Accounts_AccountNumberFrom",
                        column: x => x.AccountNumberFrom,
                        principalTable: "Accounts",
                        principalColumn: "AccountNumber",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_Accounts_AccountNumberTo",
                        column: x => x.AccountNumberTo,
                        principalTable: "Accounts",
                        principalColumn: "AccountNumber",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_AccountNumberFrom",
                table: "Transfers",
                column: "AccountNumberFrom");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_AccountNumberTo",
                table: "Transfers",
                column: "AccountNumberTo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transfers");
        }
    }
}
