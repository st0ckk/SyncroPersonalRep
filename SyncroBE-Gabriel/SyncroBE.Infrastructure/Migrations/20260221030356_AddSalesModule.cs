using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "failed_login_attempts",
                table: "users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "lockout_end",
                table: "users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "product_name",
                table: "sale_detail",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "client_id",
                table: "purchase",
                type: "varchar(20)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "purchase",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "subtotal",
                table: "purchase",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "tax_amount",
                table: "purchase",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "tax_id",
                table: "purchase",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tax_percentage",
                table: "purchase",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total",
                table: "purchase",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "line_total",
                table: "sale_detail",
                type: "decimal(18,2)",
                nullable: false,
                computedColumnSql: "[quantity] * [unit_price]",
                stored: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateTable(
                name: "tax",
                columns: table => new
                {
                    tax_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tax_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tax", x => x.tax_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sale_detail_product_id",
                table: "sale_detail",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_sale_detail_purchase_id",
                table: "sale_detail",
                column: "purchase_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_client_id",
                table: "purchase",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_tax_id",
                table: "purchase",
                column: "tax_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_user_id",
                table: "purchase",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_clients_client_id",
                table: "purchase",
                column: "client_id",
                principalTable: "clients",
                principalColumn: "client_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_tax_tax_id",
                table: "purchase",
                column: "tax_id",
                principalTable: "tax",
                principalColumn: "tax_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_sale_detail_product_product_id",
                table: "sale_detail",
                column: "product_id",
                principalTable: "product",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Restrict);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_purchase_clients_client_id",
                table: "purchase");

            migrationBuilder.DropForeignKey(
                name: "FK_purchase_tax_tax_id",
                table: "purchase");

            migrationBuilder.DropForeignKey(
                name: "FK_purchase_users_user_id",
                table: "purchase");

            migrationBuilder.DropForeignKey(
                name: "FK_sale_detail_product_product_id",
                table: "sale_detail");

            migrationBuilder.DropForeignKey(
                name: "FK_sale_detail_purchase_purchase_id",
                table: "sale_detail");

            migrationBuilder.DropTable(
                name: "tax");

            migrationBuilder.DropIndex(
                name: "IX_sale_detail_product_id",
                table: "sale_detail");

            migrationBuilder.DropIndex(
                name: "IX_sale_detail_purchase_id",
                table: "sale_detail");

            migrationBuilder.DropIndex(
                name: "IX_purchase_client_id",
                table: "purchase");

            migrationBuilder.DropIndex(
                name: "IX_purchase_tax_id",
                table: "purchase");

            migrationBuilder.DropIndex(
                name: "IX_purchase_user_id",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "failed_login_attempts",
                table: "users");

            migrationBuilder.DropColumn(
                name: "lockout_end",
                table: "users");

            migrationBuilder.DropColumn(
                name: "product_name",
                table: "sale_detail");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "subtotal",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "tax_amount",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "tax_id",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "tax_percentage",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "total",
                table: "purchase");

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "sale_detail",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "unit_price",
                table: "sale_detail",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "purchase_id",
                table: "sale_detail",
                newName: "PurchaseId");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "sale_detail",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "line_total",
                table: "sale_detail",
                newName: "LineTotal");

            migrationBuilder.RenameColumn(
                name: "sale_detail_id",
                table: "sale_detail",
                newName: "SaleDetailId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "purchase",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "purchase_paid",
                table: "purchase",
                newName: "PurchasePaid");

            migrationBuilder.RenameColumn(
                name: "purchase_date",
                table: "purchase",
                newName: "PurchaseDate");

            migrationBuilder.RenameColumn(
                name: "client_id",
                table: "purchase",
                newName: "ClientId");

            migrationBuilder.RenameColumn(
                name: "purchase_id",
                table: "purchase",
                newName: "PurchaseId");

            migrationBuilder.RenameColumn(
                name: "discount_id",
                table: "discount",
                newName: "DiscountId");

            migrationBuilder.AlterColumn<decimal>(
                name: "LineTotal",
                table: "sale_detail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComputedColumnSql: "[quantity] * [unit_price]");

            migrationBuilder.AlterColumn<int>(
                name: "ClientId",
                table: "purchase",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)");
        }
    }
}
