using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "discount_id",
                table: "quotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "quote_discountapplied",
                table: "quotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "quote_discountpercentage",
                table: "quotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "quote_discountreason",
                table: "quotes",
                type: "varchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "discount",
                columns: table => new
                {
                    discount_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    discount_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    discount_percentage = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discount", x => x.discount_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_quotes_discount_id",
                table: "quotes",
                column: "discount_id");

            migrationBuilder.AddForeignKey(
                name: "FK_quotes_discount_discount_id",
                table: "quotes",
                column: "discount_id",
                principalTable: "discount",
                principalColumn: "discount_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_quotes_discount_discount_id",
                table: "quotes");

            migrationBuilder.DropTable(
                name: "discount");

            migrationBuilder.DropIndex(
                name: "IX_quotes_discount_id",
                table: "quotes");

            migrationBuilder.DropColumn(
                name: "discount_id",
                table: "quotes");

            migrationBuilder.DropColumn(
                name: "quote_discountapplied",
                table: "quotes");

            migrationBuilder.DropColumn(
                name: "quote_discountpercentage",
                table: "quotes");

            migrationBuilder.DropColumn(
                name: "quote_discountreason",
                table: "quotes");
        }
    }
}
