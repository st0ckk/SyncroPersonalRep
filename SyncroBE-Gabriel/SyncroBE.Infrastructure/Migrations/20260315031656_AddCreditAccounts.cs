using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "client_accounts",
                columns: table => new
                {
                    clientaccount_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    client_id = table.Column<string>(type: "varchar(20)", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    clientaccount_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    clientaccount_openingdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    clientaccount_creditlimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    clientaccount_interestrate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    clientaccount_currentbalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    clientaccount_accountstatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_accounts", x => x.clientaccount_id);
                    table.ForeignKey(
                        name: "FK_client_accounts_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "client_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_client_accounts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "client_accountmovements",
                columns: table => new
                {
                    clientaccountmovement_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientaccount_id = table.Column<int>(type: "int", nullable: false),
                    clientaccountmovement_movementdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    clientaccountmovement_description = table.Column<string>(type: "varchar(max)", nullable: false),
                    clientaccountmovement_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    clientaccountmovement_newbalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    clientaccountmovement_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_accountmovements", x => x.clientaccountmovement_id);
                    table.ForeignKey(
                        name: "FK_client_accountmovements_client_accounts_clientaccount_id",
                        column: x => x.clientaccount_id,
                        principalTable: "client_accounts",
                        principalColumn: "clientaccount_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_client_accountmovements_clientaccount_id",
                table: "client_accountmovements",
                column: "clientaccount_id");

            migrationBuilder.CreateIndex(
                name: "clientaccount_number",
                table: "client_accounts",
                column: "clientaccount_number");

            migrationBuilder.CreateIndex(
                name: "IX_client_accounts_client_id",
                table: "client_accounts",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_accounts_user_id",
                table: "client_accounts",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_accountmovements");

            migrationBuilder.DropTable(
                name: "client_accounts");
        }
    }
}
