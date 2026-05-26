using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroBE.Infrastructure.Migrations
{
    public partial class AddRouteIncidents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "route_incident",
                columns: table => new
                {
                    incident_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    route_id = table.Column<int>(type: "int", nullable: true),
                    driver_user_id = table.Column<int>(type: "int", nullable: false),
                    incident_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    occurred_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_incident", x => x.incident_id);
                    table.ForeignKey(
                        name: "FK_route_incident_delivery_route_route_id",
                        column: x => x.route_id,
                        principalTable: "delivery_route",
                        principalColumn: "route_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_route_incident_users_driver_user_id",
                        column: x => x.driver_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_route_incident_driver",
                table: "route_incident",
                column: "driver_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_route_incident_route",
                table: "route_incident",
                column: "route_id",
                filter: "[route_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_route_incident_occurred_at",
                table: "route_incident",
                column: "occurred_at");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "route_incident");
        }
    }
}
