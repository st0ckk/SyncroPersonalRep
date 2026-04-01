using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPONumberField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "purchase_ordernumber",
                table: "purchase",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");


            migrationBuilder.CreateIndex(
                name: "purchase_ordernumber",
                table: "purchase",
                column: "purchase_ordernumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "purchase_ordernumber",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "purchase_ordernumber",
                table: "purchase");
        }
    }
}
