using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UserGroupService.API.Migrations
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_groups");
        }
    }
}
