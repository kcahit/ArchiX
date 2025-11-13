using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class BaseEntityDuzeltmesi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ArchiXSettings",
                type: "datetimeoffset(4)",
                precision: 4,
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(4)",
                oldPrecision: 4);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ArchiXSettings",
                type: "datetimeoffset(4)",
                precision: 4,
                nullable: false,
                defaultValueSql: "SYSDATETIMEOFFSET()");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "ArchiXSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastStatusAt",
                table: "ArchiXSettings",
                type: "datetimeoffset(4)",
                precision: 4,
                nullable: true,
                defaultValueSql: "SYSDATETIMEOFFSET()");

            migrationBuilder.AddColumn<int>(
                name: "LastStatusBy",
                table: "ArchiXSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "RowId",
                table: "ArchiXSettings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "ArchiXSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "ArchiXSettings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArchiXSettings_StatusId",
                table: "ArchiXSettings",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArchiXSettings_Status_StatusId",
                table: "ArchiXSettings",
                column: "StatusId",
                principalTable: "Status",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArchiXSettings_Status_StatusId",
                table: "ArchiXSettings");

            migrationBuilder.DropIndex(
                name: "IX_ArchiXSettings_StatusId",
                table: "ArchiXSettings");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ArchiXSettings");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ArchiXSettings");

            migrationBuilder.DropColumn(
                name: "LastStatusAt",
                table: "ArchiXSettings");

            migrationBuilder.DropColumn(
                name: "LastStatusBy",
                table: "ArchiXSettings");

            migrationBuilder.DropColumn(
                name: "RowId",
                table: "ArchiXSettings");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "ArchiXSettings");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ArchiXSettings");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ArchiXSettings",
                type: "datetimeoffset(4)",
                precision: 4,
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(4)",
                oldPrecision: 4,
                oldNullable: true);
        }
    }
}
