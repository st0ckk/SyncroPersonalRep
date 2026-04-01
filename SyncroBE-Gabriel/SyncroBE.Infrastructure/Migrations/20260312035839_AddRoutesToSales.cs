using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoutesToSales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "route_id",
                table: "purchase",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_route_id",
                table: "purchase",
                column: "route_id");

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_delivery_route_route_id",
                table: "purchase",
                column: "route_id",
                principalTable: "delivery_route",
                principalColumn: "route_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_purchase_delivery_route_route_id",
                table: "purchase");

            migrationBuilder.DropIndex(
                name: "IX_purchase_route_id",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "route_id",
                table: "purchase");
        }
    }
}
