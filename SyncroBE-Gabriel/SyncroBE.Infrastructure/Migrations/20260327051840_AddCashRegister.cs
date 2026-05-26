using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCashRegister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cash_registers",
                columns: table => new
                {
                    cashregister_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    cashregister_openingamount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    cashregister_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    cashregister_openingdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    cashregister_closingdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cashregister_expectedamount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    cashregister_reportedamount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    cashregister_amountdifference = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    cashregister_differencereason = table.Column<string>(type: "varchar(max)", nullable: true),
                    cashregister_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cash_registers", x => x.cashregister_id);
                    table.ForeignKey(
                        name: "FK_cash_registers_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cash_registermovements",
                columns: table => new
                {
                    cashregistermovement_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cashregister_id = table.Column<int>(type: "int", nullable: false),
                    PurchaseId = table.Column<int>(type: "int", nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    cashregistermovement_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    cashregistermovement_description = table.Column<string>(type: "varchar(max)", nullable: true),
                    cashregistermovement_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    cashregistermovement_manual = table.Column<bool>(type: "bit", nullable: false),
                    cashregistermovement_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cash_registermovements", x => x.cashregistermovement_id);
                    table.ForeignKey(
                        name: "FK_cash_registermovements_cash_registers_cashregister_id",
                        column: x => x.cashregister_id,
                        principalTable: "cash_registers",
                        principalColumn: "cashregister_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cash_registermovements_purchase_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "purchase",
                        principalColumn: "purchase_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cash_registermovements_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cash_registermovements_cashregister_id",
                table: "cash_registermovements",
                column: "cashregister_id");

            migrationBuilder.CreateIndex(
                name: "IX_cash_registermovements_PurchaseId",
                table: "cash_registermovements",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_cash_registermovements_user_id",
                table: "cash_registermovements",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "cashregister_number",
                table: "cash_registers",
                column: "cashregister_number");

            migrationBuilder.CreateIndex(
                name: "IX_cash_registers_user_id",
                table: "cash_registers",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cash_registermovements");

            migrationBuilder.DropTable(
                name: "cash_registers");
        }
    }
}
