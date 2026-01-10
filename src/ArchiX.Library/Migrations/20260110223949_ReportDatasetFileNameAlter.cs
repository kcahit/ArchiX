using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class ReportDatasetFileNameAlter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "ReportDatasets",
                newName: "Source");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "ReportDatasets",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(260)",
                oldMaxLength: 260);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "ReportDatasets",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3000)",
                oldMaxLength: 3000);

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "ReportDatasets",
                newName: "FileName");
        }
    }
}
