using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class ReportDatasetAlter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InputParameter",
                table: "ReportDatasets",
                type: "nvarchar(max)",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder.AddColumn<string>(
                name: "OutputParameter",
                table: "ReportDatasets",
                type: "nvarchar(max)",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 7);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InputParameter",
                table: "ReportDatasets");

            migrationBuilder.DropColumn(
                name: "OutputParameter",
                table: "ReportDatasets");
        }
    }
}
