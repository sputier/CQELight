using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CQELight.DAL.EFCore.Integration.Tests.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "AzureLocation",
                schema: "dbo",
                columns: table => new
                {
                    Country = table.Column<string>(nullable: false),
                    DataCenter = table.Column<string>(nullable: false),
                    DELETED = table.Column<bool>(nullable: false, defaultValue: false),
                    DELETE_DATE = table.Column<DateTime>(nullable: true),
                    EDIT_DATE = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AzureLocation", x => new { x.Country, x.DataCenter });
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                schema: "dbo",
                columns: table => new
                {
                    Tag_ID = table.Column<Guid>(nullable: false),
                    DELETED = table.Column<bool>(nullable: false, defaultValue: false),
                    DELETE_DATE = table.Column<DateTime>(nullable: true),
                    EDIT_DATE = table.Column<DateTime>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Tag_ID);
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "dbo",
                columns: table => new
                {
                    Use_ID = table.Column<Guid>(nullable: false),
                    DELETED = table.Column<bool>(nullable: false, defaultValue: false),
                    DELETE_DATE = table.Column<DateTime>(nullable: true),
                    EDIT_DATE = table.Column<DateTime>(nullable: false),
                    LastName = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Use_ID);
                });

            migrationBuilder.CreateTable(
                name: "WebSite",
                schema: "dbo",
                columns: table => new
                {
                    Web_ID = table.Column<Guid>(nullable: false),
                    AzureCountry = table.Column<string>(nullable: true),
                    AzureDataCenter = table.Column<string>(nullable: true),
                    DELETED = table.Column<bool>(nullable: false, defaultValue: false),
                    DELETE_DATE = table.Column<DateTime>(nullable: true),
                    EDIT_DATE = table.Column<DateTime>(nullable: false),
                    URL = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebSite", x => x.Web_ID);
                    table.ForeignKey(
                        name: "FK_WebSite_AzureLocation_AzureCountry_AzureDataCenter",
                        columns: x => new { x.AzureCountry, x.AzureDataCenter },
                        principalSchema: "dbo",
                        principalTable: "AzureLocation",
                        principalColumns: new[] { "Country", "DataCenter" },
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Hyperlinks",
                schema: "dbo",
                columns: table => new
                {
                    Hyperlink = table.Column<string>(maxLength: 1024, nullable: false),
                    DELETED = table.Column<bool>(nullable: false, defaultValue: false),
                    DELETE_DATE = table.Column<DateTime>(nullable: true),
                    EDIT_DATE = table.Column<DateTime>(nullable: false),
                    Web_ID = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hyperlinks", x => x.Hyperlink);
                    table.ForeignKey(
                        name: "FK_Hyperlinks_WebSite_Web_ID",
                        column: x => x.Web_ID,
                        principalSchema: "dbo",
                        principalTable: "WebSite",
                        principalColumn: "Web_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Post",
                schema: "dbo",
                columns: table => new
                {
                    Pos_ID = table.Column<Guid>(nullable: false),
                    Content = table.Column<string>(maxLength: 65536, nullable: false),
                    DELETED = table.Column<bool>(nullable: false, defaultValue: false),
                    DELETE_DATE = table.Column<DateTime>(nullable: true),
                    EDIT_DATE = table.Column<DateTime>(nullable: false),
                    PublicationDate = table.Column<DateTime>(nullable: true),
                    Published = table.Column<bool>(nullable: false, defaultValue: true),
                    ShortAccess = table.Column<string>(maxLength: 2048, nullable: true),
                    Version = table.Column<int>(nullable: false, defaultValue: 1),
                    Web_ID = table.Column<Guid>(nullable: false),
                    Use_ID = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Post", x => x.Pos_ID);
                    table.ForeignKey(
                        name: "FK_Post_WebSite_Web_ID",
                        column: x => x.Web_ID,
                        principalSchema: "dbo",
                        principalTable: "WebSite",
                        principalColumn: "Web_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Post_User_Use_ID",
                        column: x => x.Use_ID,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "Use_ID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Comment",
                schema: "dbo",
                columns: table => new
                {
                    Com_ID = table.Column<Guid>(nullable: false),
                    DELETED = table.Column<bool>(nullable: false, defaultValue: false),
                    DELETE_DATE = table.Column<DateTime>(nullable: true),
                    EDIT_DATE = table.Column<DateTime>(nullable: false),
                    Use_ID = table.Column<Guid>(nullable: false),
                    Pos_ID = table.Column<Guid>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.Com_ID);
                    table.ForeignKey(
                        name: "FK_Comment_User_Use_ID",
                        column: x => x.Use_ID,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "Use_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comment_Post_Pos_ID",
                        column: x => x.Pos_ID,
                        principalSchema: "dbo",
                        principalTable: "Post",
                        principalColumn: "Pos_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostTag",
                schema: "dbo",
                columns: table => new
                {
                    Pos_ID = table.Column<Guid>(nullable: false),
                    Tag_ID = table.Column<Guid>(nullable: false),
                    DELETED = table.Column<bool>(nullable: false, defaultValue: false),
                    DELETE_DATE = table.Column<DateTime>(nullable: true),
                    EDIT_DATE = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostTag", x => new { x.Pos_ID, x.Tag_ID });
                    table.ForeignKey(
                        name: "FK_PostTag_Post_Pos_ID",
                        column: x => x.Pos_ID,
                        principalSchema: "dbo",
                        principalTable: "Post",
                        principalColumn: "Pos_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostTag_Tag_Tag_ID",
                        column: x => x.Tag_ID,
                        principalSchema: "dbo",
                        principalTable: "Tag",
                        principalColumn: "Tag_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comment_Use_ID",
                schema: "dbo",
                table: "Comment",
                column: "Use_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_Pos_ID_Use_ID_Value",
                schema: "dbo",
                table: "Comment",
                columns: new[] { "Pos_ID", "Use_ID", "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_Hyperlinks_Web_ID",
                schema: "dbo",
                table: "Hyperlinks",
                column: "Web_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Post_ShortAccess",
                schema: "dbo",
                table: "Post",
                column: "ShortAccess",
                unique: true,
                filter: "[ShortAccess] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Post_Web_ID",
                schema: "dbo",
                table: "Post",
                column: "Web_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Post_Use_ID",
                schema: "dbo",
                table: "Post",
                column: "Use_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PostTag_Tag_ID",
                schema: "dbo",
                table: "PostTag",
                column: "Tag_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_Value",
                schema: "dbo",
                table: "Tag",
                column: "Value",
                unique: true,
                filter: "[Value] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WebSite_URL",
                schema: "dbo",
                table: "WebSite",
                column: "URL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebSite_AzureCountry_AzureDataCenter",
                schema: "dbo",
                table: "WebSite",
                columns: new[] { "AzureCountry", "AzureDataCenter" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comment",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Hyperlinks",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PostTag",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Post",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Tag",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "WebSite",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "User",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AzureLocation",
                schema: "dbo");
        }
    }
}
