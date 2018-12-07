using System;
using Microsoft.EntityFrameworkCore.Migrations;

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
                    EDIT_DATE = table.Column<DateTime>(nullable: false),
                    DELETED = table.Column<bool>(nullable: false, defaultValue: false),
                    DELETE_DATE = table.Column<DateTime>(nullable: true)
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
                    Tag_Id = table.Column<Guid>(nullable: false),
                    EDIT_DATE = table.Column<DateTime>(nullable: false),
                    DELETED = table.Column<bool>(nullable: false, defaultValue: false),
                    DELETE_DATE = table.Column<DateTime>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Tag_Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "dbo",
                columns: table => new
                {
                    User_Id = table.Column<Guid>(nullable: false),
                    EDIT_DATE = table.Column<DateTime>(nullable: false),
                    DELETED = table.Column<bool>(nullable: false, defaultValue: false),
                    DELETE_DATE = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.User_Id);
                });

            migrationBuilder.CreateTable(
                name: "WebSite",
                schema: "dbo",
                columns: table => new
                {
                    WebSite_Id = table.Column<Guid>(nullable: false),
                    EDIT_DATE = table.Column<DateTime>(nullable: false),
                    DELETED = table.Column<bool>(nullable: false, defaultValue: false),
                    DELETE_DATE = table.Column<DateTime>(nullable: true),
                    URL = table.Column<string>(nullable: false),
                    AzureCountry = table.Column<string>(nullable: true),
                    AzureDataCenter = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebSite", x => x.WebSite_Id);
                    table.ForeignKey(
                        name: "FK_WebSite_AzureLocation_AzureCountry_AzureDataCenter",
                        columns: x => new { x.AzureCountry, x.AzureDataCenter },
                        principalSchema: "dbo",
                        principalTable: "AzureLocation",
                        principalColumns: new[] { "Country", "DataCenter" },
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Word",
                schema: "dbo",
                columns: table => new
                {
                    Word_Id = table.Column<string>(nullable: false),
                    Tag_Id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Word", x => x.Word_Id);
                    table.ForeignKey(
                        name: "FK_Word_Tag_Tag_Id",
                        column: x => x.Tag_Id,
                        principalSchema: "dbo",
                        principalTable: "Tag",
                        principalColumn: "Tag_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Hyperlinks",
                schema: "dbo",
                columns: table => new
                {
                    Hyperlink = table.Column<string>(maxLength: 1024, nullable: false),
                    EDIT_DATE = table.Column<DateTime>(nullable: false),
                    DELETED = table.Column<bool>(nullable: false, defaultValue: false),
                    DELETE_DATE = table.Column<DateTime>(nullable: true),
                    WebSite_Id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hyperlinks", x => x.Hyperlink);
                    table.ForeignKey(
                        name: "FK_Hyperlinks_WebSite_WebSite_Id",
                        column: x => x.WebSite_Id,
                        principalSchema: "dbo",
                        principalTable: "WebSite",
                        principalColumn: "WebSite_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Post",
                schema: "dbo",
                columns: table => new
                {
                    Post_Id = table.Column<Guid>(nullable: false),
                    EDIT_DATE = table.Column<DateTime>(nullable: false),
                    DELETED = table.Column<bool>(nullable: false, defaultValue: false),
                    DELETE_DATE = table.Column<DateTime>(nullable: true),
                    Content = table.Column<string>(maxLength: 65536, nullable: false),
                    ShortAccess = table.Column<string>(maxLength: 2048, nullable: true),
                    Version = table.Column<int>(nullable: false, defaultValue: 1),
                    Published = table.Column<bool>(nullable: false, defaultValue: true),
                    PublicationDate = table.Column<DateTime>(nullable: true),
                    User_Id = table.Column<Guid>(nullable: true),
                    WebSite_Id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Post", x => x.Post_Id);
                    table.ForeignKey(
                        name: "FK_Post_WebSite_WebSite_Id",
                        column: x => x.WebSite_Id,
                        principalSchema: "dbo",
                        principalTable: "WebSite",
                        principalColumn: "WebSite_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Post_User_User_Id",
                        column: x => x.User_Id,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "User_Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Comment",
                schema: "dbo",
                columns: table => new
                {
                    Comment_Id = table.Column<Guid>(nullable: false),
                    EDIT_DATE = table.Column<DateTime>(nullable: false),
                    DELETED = table.Column<bool>(nullable: false, defaultValue: false),
                    DELETE_DATE = table.Column<DateTime>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    User_Id = table.Column<Guid>(nullable: false),
                    Post_Id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.Comment_Id);
                    table.ForeignKey(
                        name: "FK_Comment_User_User_Id",
                        column: x => x.User_Id,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "User_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comment_Post_Post_Id",
                        column: x => x.Post_Id,
                        principalSchema: "dbo",
                        principalTable: "Post",
                        principalColumn: "Post_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostTag",
                schema: "dbo",
                columns: table => new
                {
                    Post_Id = table.Column<Guid>(nullable: false),
                    Tag_Id = table.Column<Guid>(nullable: false),
                    EDIT_DATE = table.Column<DateTime>(nullable: false),
                    DELETED = table.Column<bool>(nullable: false, defaultValue: false),
                    DELETE_DATE = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostTag", x => new { x.Post_Id, x.Tag_Id });
                    table.ForeignKey(
                        name: "FK_PostTag_Post_Post_Id",
                        column: x => x.Post_Id,
                        principalSchema: "dbo",
                        principalTable: "Post",
                        principalColumn: "Post_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostTag_Tag_Tag_Id",
                        column: x => x.Tag_Id,
                        principalSchema: "dbo",
                        principalTable: "Tag",
                        principalColumn: "Tag_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comment_User_Id",
                schema: "dbo",
                table: "Comment",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_Post_Id_User_Id_Value",
                schema: "dbo",
                table: "Comment",
                columns: new[] { "Post_Id", "User_Id", "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_Hyperlinks_WebSite_Id",
                schema: "dbo",
                table: "Hyperlinks",
                column: "WebSite_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Post_ShortAccess",
                schema: "dbo",
                table: "Post",
                column: "ShortAccess",
                unique: true,
                filter: "[ShortAccess] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Post_WebSite_Id",
                schema: "dbo",
                table: "Post",
                column: "WebSite_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Post_User_Id",
                schema: "dbo",
                table: "Post",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_PostTag_Tag_Id",
                schema: "dbo",
                table: "PostTag",
                column: "Tag_Id");

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

            migrationBuilder.CreateIndex(
                name: "IX_Word_Tag_Id",
                schema: "dbo",
                table: "Word",
                column: "Tag_Id");
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
                name: "Word",
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
