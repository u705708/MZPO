using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MZPO.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AmoAccounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: false),
                    subdomain = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: false),
                    client_id = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    client_secret = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    redirect_uri = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    code = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    authToken = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    refrToken = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    validity = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmoAccounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "CFs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    AmoId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: false),
                    EntityName = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CFs", x => new { x.Id, x.AmoId });
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    EngName = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: false),
                    RusName = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.EngName);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    AmoId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: false),
                    EntityName = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => new { x.Id, x.AmoId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmoAccounts");

            migrationBuilder.DropTable(
                name: "CFs");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Tags");
        }
    }
}
