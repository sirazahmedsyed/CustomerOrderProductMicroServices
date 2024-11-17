using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UserService.API.Migrations
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

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserNo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserLocalName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserForeignName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UserGroupNo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserNo);
                    table.ForeignKey(
                        name: "FK_Users_UserGroups_UserGroupNo",
                        column: x => x.UserGroupNo,
                        principalTable: "UserGroups",
                        principalColumn: "UserGroupNo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserGroupNo",
                table: "Users",
                column: "UserGroupNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "UserGroups");
        }
    }
}
