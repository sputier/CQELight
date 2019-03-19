using Microsoft.EntityFrameworkCore.Migrations;

namespace CQELight.DAL.EFCore.Integration.Tests.Migrations
{
    public partial class IPersistableEntityComposedKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComposedKeyEntity",
                columns: table => new
                {
                    FirstPart = table.Column<string>(nullable: false),
                    SecondPart = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComposedKeyEntity", x => new { x.FirstPart, x.SecondPart });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComposedKeyEntity");
        }
    }
}
