using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientAccountToPurchases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "clientaccount_id",
                table: "purchase",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_clientaccount_id",
                table: "purchase",
                column: "clientaccount_id");

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_client_accounts_clientaccount_id",
                table: "purchase",
                column: "clientaccount_id",
                principalTable: "client_accounts",
                principalColumn: "clientaccount_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_purchase_client_accounts_clientaccount_id",
                table: "purchase");

            migrationBuilder.DropIndex(
                name: "IX_purchase_clientaccount_id",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "clientaccount_id",
                table: "purchase");
        }
    }
}
