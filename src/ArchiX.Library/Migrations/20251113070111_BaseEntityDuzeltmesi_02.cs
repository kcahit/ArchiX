using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class BaseEntityDuzeltmesi_02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ConnectionServerWhitelist",
                table: "ConnectionServerWhitelist");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConnectionAudit",
                table: "ConnectionAudit");

            migrationBuilder.DropColumn(
                name: "AddedAt",
                table: "ConnectionServerWhitelist");

            migrationBuilder.RenameTable(
                name: "ConnectionServerWhitelist",
                newName: "ConnectionServerWhitelists");

            migrationBuilder.RenameTable(
                name: "ConnectionAudit",
                newName: "ConnectionAudits");

            migrationBuilder.RenameIndex(
                name: "IX_ConnectionServerWhitelist_ServerName_IsActive",
                table: "ConnectionServerWhitelists",
                newName: "IX_ConnectionServerWhitelists_ServerName_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_ConnectionServerWhitelist_EnvScope",
                table: "ConnectionServerWhitelists",
                newName: "IX_ConnectionServerWhitelists_EnvScope");

            migrationBuilder.RenameIndex(
                name: "IX_ConnectionServerWhitelist_Cidr_IsActive",
                table: "ConnectionServerWhitelists",
                newName: "IX_ConnectionServerWhitelists_Cidr_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_ConnectionAudit_Result",
                table: "ConnectionAudits",
                newName: "IX_ConnectionAudits_Result");

            migrationBuilder.RenameIndex(
                name: "IX_ConnectionAudit_CorrelationId",
                table: "ConnectionAudits",
                newName: "IX_ConnectionAudits_CorrelationId");

            migrationBuilder.RenameIndex(
                name: "IX_ConnectionAudit_AttemptedAt",
                table: "ConnectionAudits",
                newName: "IX_ConnectionAudits_AttemptedAt");

            migrationBuilder.AddColumn<bool>(
                name: "IsProtected",
                table: "Status",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsProtected",
                table: "LanguagePacks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsProtected",
                table: "FilterItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ConnectionServerWhitelists",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ConnectionServerWhitelists",
                type: "datetimeoffset(4)",
                precision: 4,
                nullable: false,
                defaultValueSql: "SYSDATETIMEOFFSET()");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "ConnectionServerWhitelists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsProtected",
                table: "ConnectionServerWhitelists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastStatusAt",
                table: "ConnectionServerWhitelists",
                type: "datetimeoffset(4)",
                precision: 4,
                nullable: true,
                defaultValueSql: "SYSDATETIMEOFFSET()");

            migrationBuilder.AddColumn<int>(
                name: "LastStatusBy",
                table: "ConnectionServerWhitelists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "RowId",
                table: "ConnectionServerWhitelists",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "ConnectionServerWhitelists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ConnectionServerWhitelists",
                type: "datetimeoffset(4)",
                precision: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "ConnectionServerWhitelists",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ConnectionAudits",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ConnectionAudits",
                type: "datetimeoffset(4)",
                precision: 4,
                nullable: false,
                defaultValueSql: "SYSDATETIMEOFFSET()");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "ConnectionAudits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsProtected",
                table: "ConnectionAudits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastStatusAt",
                table: "ConnectionAudits",
                type: "datetimeoffset(4)",
                precision: 4,
                nullable: true,
                defaultValueSql: "SYSDATETIMEOFFSET()");

            migrationBuilder.AddColumn<int>(
                name: "LastStatusBy",
                table: "ConnectionAudits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "RowId",
                table: "ConnectionAudits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "ConnectionAudits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ConnectionAudits",
                type: "datetimeoffset(4)",
                precision: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "ConnectionAudits",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConnectionServerWhitelists",
                table: "ConnectionServerWhitelists",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConnectionAudits",
                table: "ConnectionAudits",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionServerWhitelists_StatusId",
                table: "ConnectionServerWhitelists",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionAudits_StatusId",
                table: "ConnectionAudits",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConnectionAudits_Status_StatusId",
                table: "ConnectionAudits",
                column: "StatusId",
                principalTable: "Status",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConnectionServerWhitelists_Status_StatusId",
                table: "ConnectionServerWhitelists",
                column: "StatusId",
                principalTable: "Status",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConnectionAudits_Status_StatusId",
                table: "ConnectionAudits");

            migrationBuilder.DropForeignKey(
                name: "FK_ConnectionServerWhitelists_Status_StatusId",
                table: "ConnectionServerWhitelists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConnectionServerWhitelists",
                table: "ConnectionServerWhitelists");

            migrationBuilder.DropIndex(
                name: "IX_ConnectionServerWhitelists_StatusId",
                table: "ConnectionServerWhitelists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConnectionAudits",
                table: "ConnectionAudits");

            migrationBuilder.DropIndex(
                name: "IX_ConnectionAudits_StatusId",
                table: "ConnectionAudits");

            migrationBuilder.DropColumn(
                name: "IsProtected",
                table: "Status");

            migrationBuilder.DropColumn(
                name: "IsProtected",
                table: "LanguagePacks");

            migrationBuilder.DropColumn(
                name: "IsProtected",
                table: "FilterItems");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ConnectionServerWhitelists");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ConnectionServerWhitelists");

            migrationBuilder.DropColumn(
                name: "IsProtected",
                table: "ConnectionServerWhitelists");

            migrationBuilder.DropColumn(
                name: "LastStatusAt",
                table: "ConnectionServerWhitelists");

            migrationBuilder.DropColumn(
                name: "LastStatusBy",
                table: "ConnectionServerWhitelists");

            migrationBuilder.DropColumn(
                name: "RowId",
                table: "ConnectionServerWhitelists");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "ConnectionServerWhitelists");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ConnectionServerWhitelists");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ConnectionServerWhitelists");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ConnectionAudits");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ConnectionAudits");

            migrationBuilder.DropColumn(
                name: "IsProtected",
                table: "ConnectionAudits");

            migrationBuilder.DropColumn(
                name: "LastStatusAt",
                table: "ConnectionAudits");

            migrationBuilder.DropColumn(
                name: "LastStatusBy",
                table: "ConnectionAudits");

            migrationBuilder.DropColumn(
                name: "RowId",
                table: "ConnectionAudits");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "ConnectionAudits");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ConnectionAudits");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ConnectionAudits");

            migrationBuilder.RenameTable(
                name: "ConnectionServerWhitelists",
                newName: "ConnectionServerWhitelist");

            migrationBuilder.RenameTable(
                name: "ConnectionAudits",
                newName: "ConnectionAudit");

            migrationBuilder.RenameIndex(
                name: "IX_ConnectionServerWhitelists_ServerName_IsActive",
                table: "ConnectionServerWhitelist",
                newName: "IX_ConnectionServerWhitelist_ServerName_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_ConnectionServerWhitelists_EnvScope",
                table: "ConnectionServerWhitelist",
                newName: "IX_ConnectionServerWhitelist_EnvScope");

            migrationBuilder.RenameIndex(
                name: "IX_ConnectionServerWhitelists_Cidr_IsActive",
                table: "ConnectionServerWhitelist",
                newName: "IX_ConnectionServerWhitelist_Cidr_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_ConnectionAudits_Result",
                table: "ConnectionAudit",
                newName: "IX_ConnectionAudit_Result");

            migrationBuilder.RenameIndex(
                name: "IX_ConnectionAudits_CorrelationId",
                table: "ConnectionAudit",
                newName: "IX_ConnectionAudit_CorrelationId");

            migrationBuilder.RenameIndex(
                name: "IX_ConnectionAudits_AttemptedAt",
                table: "ConnectionAudit",
                newName: "IX_ConnectionAudit_AttemptedAt");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "ConnectionServerWhitelist",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AddedAt",
                table: "ConnectionServerWhitelist",
                type: "datetimeoffset(4)",
                precision: 4,
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "ConnectionAudit",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConnectionServerWhitelist",
                table: "ConnectionServerWhitelist",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConnectionAudit",
                table: "ConnectionAudit",
                column: "Id");
        }
    }
}
