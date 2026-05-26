using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScreenPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "screen_permissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    screen_key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    granted_by = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_screen_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_screen_permissions_users_granted_by",
                        column: x => x.granted_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_screen_permissions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_screen_permissions_user",
                table: "screen_permissions",
                column: "user_id",
                filter: "[user_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_screen_permissions_granted_by",
                table: "screen_permissions",
                column: "granted_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "screen_permissions");
        }
    }
}
