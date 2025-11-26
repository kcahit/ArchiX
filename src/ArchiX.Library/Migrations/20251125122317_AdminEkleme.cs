using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiX.Library.src.ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class AdminEkleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedBy", "DisplayName", "Email", "IsAdmin", "IsProtected", "LastStatusBy", "NormalizedEmail", "NormalizedUserName", "Phone", "StatusId", "UpdatedAt", "UpdatedBy", "UserName" },
                values: new object[] { 1, 0, "System Admin", "admin@example.com", true, false, 0, "ADMIN@EXAMPLE.COM", "ADMIN", null, 3, null, null, "admin" });

            migrationBuilder.InsertData(
                table: "UserApplications",
                columns: new[] { "Id", "ApplicationId", "CreatedBy", "IsProtected", "LastStatusBy", "StatusId", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[] { 1, 1, 0, false, 0, 3, null, null, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserApplications",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
