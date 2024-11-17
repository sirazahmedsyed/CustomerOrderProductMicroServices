using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UserGroupService.API.Migrations
{
    /// <inheritdoc />
    public partial class intialcreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserGroups",
                columns: table => new
                {
                    UserGroupNo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserGroupLocalName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserGroupForeignName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AllowAddUser = table.Column<short>(type: "int2", nullable: false),
                    AllowAddUserGroup = table.Column<short>(type: "int2", nullable: false),
                    AllowAddCustomer = table.Column<short>(type: "int2", nullable: false),
                    AllowAddProducts = table.Column<short>(type: "int2", nullable: false),
                    AllowAddOrder = table.Column<short>(type: "int2", nullable: false),
                    InactiveFlag = table.Column<short>(type: "int2", nullable: false),
                    IsAdmin = table.Column<short>(type: "int2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroups", x => x.UserGroupNo);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGroups");
        }
    }
}
