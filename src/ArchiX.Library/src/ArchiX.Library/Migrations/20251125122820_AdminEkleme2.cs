using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiX.Library.src.ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class AdminEkleme2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserApplications",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsProtected",
                value: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsProtected",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserApplications",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsProtected",
                value: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsProtected",
                value: false);
        }
    }
}
