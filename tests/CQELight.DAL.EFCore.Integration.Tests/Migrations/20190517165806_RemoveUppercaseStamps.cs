using Microsoft.EntityFrameworkCore.Migrations;

namespace CQELight.DAL.EFCore.Integration.Tests.Migrations
{
    public partial class RemoveUppercaseStamps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DELETED",
                table: "WebSite",
                newName: "Deleted");

            migrationBuilder.RenameColumn(
                name: "EDIT_DATE",
                table: "WebSite",
                newName: "EditDate");

            migrationBuilder.RenameColumn(
                name: "DELETE_DATE",
                table: "WebSite",
                newName: "DeleteDate");

            migrationBuilder.RenameColumn(
                name: "DELETED",
                table: "User",
                newName: "Deleted");

            migrationBuilder.RenameColumn(
                name: "EDIT_DATE",
                table: "User",
                newName: "EditDate");

            migrationBuilder.RenameColumn(
                name: "DELETE_DATE",
                table: "User",
                newName: "DeleteDate");

            migrationBuilder.RenameColumn(
                name: "DELETED",
                table: "Tag",
                newName: "Deleted");

            migrationBuilder.RenameColumn(
                name: "EDIT_DATE",
                table: "Tag",
                newName: "EditDate");

            migrationBuilder.RenameColumn(
                name: "DELETE_DATE",
                table: "Tag",
                newName: "DeleteDate");

            migrationBuilder.RenameColumn(
                name: "DELETED",
                table: "PostTag",
                newName: "Deleted");

            migrationBuilder.RenameColumn(
                name: "EDIT_DATE",
                table: "PostTag",
                newName: "EditDate");

            migrationBuilder.RenameColumn(
                name: "DELETE_DATE",
                table: "PostTag",
                newName: "DeleteDate");

            migrationBuilder.RenameColumn(
                name: "DELETED",
                table: "Post",
                newName: "Deleted");

            migrationBuilder.RenameColumn(
                name: "EDIT_DATE",
                table: "Post",
                newName: "EditDate");

            migrationBuilder.RenameColumn(
                name: "DELETE_DATE",
                table: "Post",
                newName: "DeleteDate");

            migrationBuilder.RenameColumn(
                name: "DELETED",
                table: "Hyperlinks",
                newName: "Deleted");

            migrationBuilder.RenameColumn(
                name: "EDIT_DATE",
                table: "Hyperlinks",
                newName: "EditDate");

            migrationBuilder.RenameColumn(
                name: "DELETE_DATE",
                table: "Hyperlinks",
                newName: "DeleteDate");

            migrationBuilder.RenameColumn(
                name: "DELETED",
                table: "Comment",
                newName: "Deleted");

            migrationBuilder.RenameColumn(
                name: "EDIT_DATE",
                table: "Comment",
                newName: "EditDate");

            migrationBuilder.RenameColumn(
                name: "DELETE_DATE",
                table: "Comment",
                newName: "DeleteDate");

            migrationBuilder.RenameColumn(
                name: "DELETED",
                table: "AzureLocation",
                newName: "Deleted");

            migrationBuilder.RenameColumn(
                name: "EDIT_DATE",
                table: "AzureLocation",
                newName: "EditDate");

            migrationBuilder.RenameColumn(
                name: "DELETE_DATE",
                table: "AzureLocation",
                newName: "DeleteDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Deleted",
                table: "WebSite",
                newName: "DELETED");

            migrationBuilder.RenameColumn(
                name: "EditDate",
                table: "WebSite",
                newName: "EDIT_DATE");

            migrationBuilder.RenameColumn(
                name: "DeleteDate",
                table: "WebSite",
                newName: "DELETE_DATE");

            migrationBuilder.RenameColumn(
                name: "Deleted",
                table: "User",
                newName: "DELETED");

            migrationBuilder.RenameColumn(
                name: "EditDate",
                table: "User",
                newName: "EDIT_DATE");

            migrationBuilder.RenameColumn(
                name: "DeleteDate",
                table: "User",
                newName: "DELETE_DATE");

            migrationBuilder.RenameColumn(
                name: "Deleted",
                table: "Tag",
                newName: "DELETED");

            migrationBuilder.RenameColumn(
                name: "EditDate",
                table: "Tag",
                newName: "EDIT_DATE");

            migrationBuilder.RenameColumn(
                name: "DeleteDate",
                table: "Tag",
                newName: "DELETE_DATE");

            migrationBuilder.RenameColumn(
                name: "Deleted",
                table: "PostTag",
                newName: "DELETED");

            migrationBuilder.RenameColumn(
                name: "EditDate",
                table: "PostTag",
                newName: "EDIT_DATE");

            migrationBuilder.RenameColumn(
                name: "DeleteDate",
                table: "PostTag",
                newName: "DELETE_DATE");

            migrationBuilder.RenameColumn(
                name: "Deleted",
                table: "Post",
                newName: "DELETED");

            migrationBuilder.RenameColumn(
                name: "EditDate",
                table: "Post",
                newName: "EDIT_DATE");

            migrationBuilder.RenameColumn(
                name: "DeleteDate",
                table: "Post",
                newName: "DELETE_DATE");

            migrationBuilder.RenameColumn(
                name: "Deleted",
                table: "Hyperlinks",
                newName: "DELETED");

            migrationBuilder.RenameColumn(
                name: "EditDate",
                table: "Hyperlinks",
                newName: "EDIT_DATE");

            migrationBuilder.RenameColumn(
                name: "DeleteDate",
                table: "Hyperlinks",
                newName: "DELETE_DATE");

            migrationBuilder.RenameColumn(
                name: "Deleted",
                table: "Comment",
                newName: "DELETED");

            migrationBuilder.RenameColumn(
                name: "EditDate",
                table: "Comment",
                newName: "EDIT_DATE");

            migrationBuilder.RenameColumn(
                name: "DeleteDate",
                table: "Comment",
                newName: "DELETE_DATE");

            migrationBuilder.RenameColumn(
                name: "Deleted",
                table: "AzureLocation",
                newName: "DELETED");

            migrationBuilder.RenameColumn(
                name: "EditDate",
                table: "AzureLocation",
                newName: "EDIT_DATE");

            migrationBuilder.RenameColumn(
                name: "DeleteDate",
                table: "AzureLocation",
                newName: "DELETE_DATE");
        }
    }
}
