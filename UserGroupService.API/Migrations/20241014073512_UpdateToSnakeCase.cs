using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserGroupService.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserGroups",
                table: "UserGroups");

            migrationBuilder.RenameTable(
                name: "UserGroups",
                newName: "user_groups");

            migrationBuilder.RenameColumn(
                name: "UserGroupLocalName",
                table: "user_groups",
                newName: "user_group_local_name");

            migrationBuilder.RenameColumn(
                name: "UserGroupForeignName",
                table: "user_groups",
                newName: "user_group_foreign_name");

            migrationBuilder.RenameColumn(
                name: "IsAdmin",
                table: "user_groups",
                newName: "is_admin");

            migrationBuilder.RenameColumn(
                name: "InactiveFlag",
                table: "user_groups",
                newName: "inactive_flag");

            migrationBuilder.RenameColumn(
                name: "AllowAddUserGroup",
                table: "user_groups",
                newName: "allow_add_user_group");

            migrationBuilder.RenameColumn(
                name: "AllowAddUser",
                table: "user_groups",
                newName: "allow_add_user");

            migrationBuilder.RenameColumn(
                name: "AllowAddProducts",
                table: "user_groups",
                newName: "allow_add_products");

            migrationBuilder.RenameColumn(
                name: "AllowAddOrder",
                table: "user_groups",
                newName: "allow_add_order");

            migrationBuilder.RenameColumn(
                name: "AllowAddCustomer",
                table: "user_groups",
                newName: "allow_add_customer");

            migrationBuilder.RenameColumn(
                name: "UserGroupNo",
                table: "user_groups",
                newName: "user_group_no");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_groups",
                table: "user_groups",
                column: "user_group_no");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_groups",
                table: "user_groups");

            migrationBuilder.RenameTable(
                name: "user_groups",
                newName: "UserGroups");

            migrationBuilder.RenameColumn(
                name: "user_group_local_name",
                table: "UserGroups",
                newName: "UserGroupLocalName");

            migrationBuilder.RenameColumn(
                name: "user_group_foreign_name",
                table: "UserGroups",
                newName: "UserGroupForeignName");

            migrationBuilder.RenameColumn(
                name: "is_admin",
                table: "UserGroups",
                newName: "IsAdmin");

            migrationBuilder.RenameColumn(
                name: "inactive_flag",
                table: "UserGroups",
                newName: "InactiveFlag");

            migrationBuilder.RenameColumn(
                name: "allow_add_user_group",
                table: "UserGroups",
                newName: "AllowAddUserGroup");

            migrationBuilder.RenameColumn(
                name: "allow_add_user",
                table: "UserGroups",
                newName: "AllowAddUser");

            migrationBuilder.RenameColumn(
                name: "allow_add_products",
                table: "UserGroups",
                newName: "AllowAddProducts");

            migrationBuilder.RenameColumn(
                name: "allow_add_order",
                table: "UserGroups",
                newName: "AllowAddOrder");

            migrationBuilder.RenameColumn(
                name: "allow_add_customer",
                table: "UserGroups",
                newName: "AllowAddCustomer");

            migrationBuilder.RenameColumn(
                name: "user_group_no",
                table: "UserGroups",
                newName: "UserGroupNo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserGroups",
                table: "UserGroups",
                column: "UserGroupNo");
        }
    }
}
