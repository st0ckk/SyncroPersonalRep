using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountToSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "discount_id",
                table: "purchase",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "purchase_discountapplied",
                table: "purchase",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "purchase_discountpercentage",
                table: "purchase",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "purchase_discountreason",
                table: "purchase",
                type: "varchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_discount_id",
                table: "purchase",
                column: "discount_id");

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_discount_discount_id",
                table: "purchase",
                column: "discount_id",
                principalTable: "discount",
                principalColumn: "discount_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_purchase_discount_discount_id",
                table: "purchase");

            migrationBuilder.DropIndex(
                name: "IX_purchase_discount_id",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "discount_id",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "purchase_discountapplied",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "purchase_discountpercentage",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "purchase_discountreason",
                table: "purchase");
        }
    }
}
