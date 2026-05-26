using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegisterToSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {    
            migrationBuilder.AddColumn<int>(
                name: "cashregister_id",
                table: "purchase",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_cashregister_id",
                table: "purchase",
                column: "cashregister_id");

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_cash_registers_cashregister_id",
                table: "purchase",
                column: "cashregister_id",
                principalTable: "cash_registers",
                principalColumn: "cashregister_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_purchase_cash_registers_cashregister_id",
                table: "purchase");

            migrationBuilder.DropIndex(
                name: "IX_purchase_cashregister_id",
                table: "purchase");
        }
    }
}
