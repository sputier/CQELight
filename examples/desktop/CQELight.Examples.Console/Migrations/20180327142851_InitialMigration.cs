using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CQELight.Examples.Console.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "MES_T_MESSAGE",
                schema: "dbo",
                columns: table => new
                {
                    MES_ID = table.Column<Guid>(nullable: false),
                    DELETED = table.Column<bool>(nullable: false, defaultValue: false),
                    DELETE_DATE = table.Column<DateTime>(nullable: true),
                    EDIT_DATE = table.Column<DateTime>(nullable: false),
                    MES_MESSAGE = table.Column<string>(nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_MES_T_MESSAGE", x => x.MES_ID));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MES_T_MESSAGE",
                schema: "dbo");
        }
    }
}
