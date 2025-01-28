using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AuditSrvice.API.Migrations
{
    /// <inheritdoc />
    public partial class intialcreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit",
                columns: table => new
                {
                    audt_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    scr_nm = table.Column<string>(type: "text", nullable: false),
                    obj_nm = table.Column<string>(type: "text", nullable: false),
                    scr_pk = table.Column<Guid>(type: "uuid", nullable: false),
                    audt_json = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit", x => x.audt_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit");
        }
    }
}
