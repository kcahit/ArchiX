using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class EnforceRestrictDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserApplications_Applications_ApplicationId",
                table: "UserApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserApplications_Users_UserId",
                table: "UserApplications");

            migrationBuilder.AddForeignKey(
                name: "FK_UserApplications_Applications_ApplicationId",
                table: "UserApplications",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserApplications_Users_UserId",
                table: "UserApplications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserApplications_Applications_ApplicationId",
                table: "UserApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserApplications_Users_UserId",
                table: "UserApplications");

            migrationBuilder.AddForeignKey(
                name: "FK_UserApplications_Applications_ApplicationId",
                table: "UserApplications",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserApplications_Users_UserId",
                table: "UserApplications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
