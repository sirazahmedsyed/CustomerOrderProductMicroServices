using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UserService.API.Migrations
{
    /// <inheritdoc />
    public partial class intialcreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_groups",
                columns: table => new
                {
                    user_group_no = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_group_local_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    user_group_foreign_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    allow_add_user = table.Column<short>(type: "int2", nullable: false),
                    allow_add_user_group = table.Column<short>(type: "int2", nullable: false),
                    allow_add_customer = table.Column<short>(type: "int2", nullable: false),
                    allow_add_products = table.Column<short>(type: "int2", nullable: false),
                    allow_add_order = table.Column<short>(type: "int2", nullable: false),
                    inactive_flag = table.Column<short>(type: "int2", nullable: false),
                    is_admin = table.Column<short>(type: "int2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_groups", x => x.user_group_no);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_no = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_local_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    user_foreign_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    user_group_no = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_no);
                    table.ForeignKey(
                        name: "FK_users_user_groups_user_group_no",
                        column: x => x.user_group_no,
                        principalTable: "user_groups",
                        principalColumn: "user_group_no",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_user_group_no",
                table: "users",
                column: "user_group_no");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "user_groups");
        }
    }
}
