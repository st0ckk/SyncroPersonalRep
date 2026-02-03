using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "distributor",
                columns: table => new
                {
                    distributor_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    distributor_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_distributor", x => x.distributor_id);
                });

            migrationBuilder.CreateTable(
                name: "province",
                columns: table => new
                {
                    province_code = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    province_name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_province", x => x.province_code);
                });

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    PurchaseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PurchasePaid = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.PurchaseId);
                });

            migrationBuilder.CreateTable(
                name: "SaleDetails",
                columns: table => new
                {
                    SaleDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleDetails", x => x.SaleDetailId);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    user_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    user_lastname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    user_email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_login = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    distributor_id = table.Column<int>(type: "int", nullable: false),
                    product_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    product_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    product_price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    product_quantity = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_product_distributor_distributor_id",
                        column: x => x.distributor_id,
                        principalTable: "distributor",
                        principalColumn: "distributor_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "canton",
                columns: table => new
                {
                    canton_code = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    canton_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    province_code = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_canton", x => x.canton_code);
                    table.ForeignKey(
                        name: "FK_canton_province_province_code",
                        column: x => x.province_code,
                        principalTable: "province",
                        principalColumn: "province_code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "district",
                columns: table => new
                {
                    district_code = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    district_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    canton_code = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_district", x => x.district_code);
                    table.ForeignKey(
                        name: "FK_district_canton_canton_code",
                        column: x => x.canton_code,
                        principalTable: "canton",
                        principalColumn: "canton_code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    client_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    client_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    client_email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    client_phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    client_electronic_invoice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    client_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    client_purchases = table.Column<int>(type: "int", nullable: false),
                    client_last_purchase = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    province_code = table.Column<int>(type: "int", nullable: true),
                    canton_code = table.Column<int>(type: "int", nullable: true),
                    district_code = table.Column<int>(type: "int", nullable: true),
                    exact_address = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.client_id);
                    table.ForeignKey(
                        name: "FK_clients_canton_canton_code",
                        column: x => x.canton_code,
                        principalTable: "canton",
                        principalColumn: "canton_code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_clients_district_district_code",
                        column: x => x.district_code,
                        principalTable: "district",
                        principalColumn: "district_code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_clients_province_province_code",
                        column: x => x.province_code,
                        principalTable: "province",
                        principalColumn: "province_code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "client_location",
                columns: table => new
                {
                    location_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    client_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_location", x => x.location_id);
                    table.ForeignKey(
                        name: "FK_client_location_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "client_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quotes",
                columns: table => new
                {
                    quote_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    client_id = table.Column<string>(type: "varchar(20)", nullable: false),
                    quote_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    quote_customer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    quote_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    quote_validdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    quote_remarks = table.Column<string>(type: "varchar(max)", nullable: false),
                    quote_conditions = table.Column<string>(type: "varchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quotes", x => x.quote_id);
                    table.ForeignKey(
                        name: "FK_quotes_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "client_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_quotes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quote_detail",
                columns: table => new
                {
                    QuoteDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    quote_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    product_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    line_total = table.Column<decimal>(type: "decimal(18,2)", nullable: false, computedColumnSql: "[quantity] * [unit_price]", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quote_detail", x => x.QuoteDetailId);
                    table.ForeignKey(
                        name: "FK_quote_detail_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_quote_detail_quotes_quote_id",
                        column: x => x.quote_id,
                        principalTable: "quotes",
                        principalColumn: "quote_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_canton_province_code",
                table: "canton",
                column: "province_code");

            migrationBuilder.CreateIndex(
                name: "IX_client_location_client_id",
                table: "client_location",
                column: "client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clients_canton_code",
                table: "clients",
                column: "canton_code");

            migrationBuilder.CreateIndex(
                name: "IX_clients_district_code",
                table: "clients",
                column: "district_code");

            migrationBuilder.CreateIndex(
                name: "IX_clients_province_code",
                table: "clients",
                column: "province_code");

            migrationBuilder.CreateIndex(
                name: "IX_distributor_distributor_code",
                table: "distributor",
                column: "distributor_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_district_canton_code",
                table: "district",
                column: "canton_code");

            migrationBuilder.CreateIndex(
                name: "IX_product_distributor_id",
                table: "product",
                column: "distributor_id");

            migrationBuilder.CreateIndex(
                name: "IX_quote_detail_product_id",
                table: "quote_detail",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_quote_detail_quote_id",
                table: "quote_detail",
                column: "quote_id");

            migrationBuilder.CreateIndex(
                name: "IX_quotes_client_id",
                table: "quotes",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_quotes_user_id",
                table: "quotes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "quote_number",
                table: "quotes",
                column: "quote_number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_location");

            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropTable(
                name: "quote_detail");

            migrationBuilder.DropTable(
                name: "SaleDetails");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "quotes");

            migrationBuilder.DropTable(
                name: "distributor");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "district");

            migrationBuilder.DropTable(
                name: "canton");

            migrationBuilder.DropTable(
                name: "province");
        }
    }
}
