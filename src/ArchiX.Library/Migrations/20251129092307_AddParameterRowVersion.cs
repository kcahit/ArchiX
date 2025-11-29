using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class AddParameterRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Parameters",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PasswordPolicyAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OldJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    LastStatusAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: true, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    LastStatusBy = table.Column<int>(type: "int", nullable: false),
                    IsProtected = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordPolicyAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordPolicyAudits_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Parameters",
                keyColumn: "Id",
                keyValue: 1,
                column: "Value",
                value: "{\n  \"defaultChannel\": \"Sms\"\n}");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordPolicyAudits_ApplicationId",
                table: "PasswordPolicyAudits",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordPolicyAudits_StatusId",
                table: "PasswordPolicyAudits",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordPolicyAudits");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Parameters");

            migrationBuilder.UpdateData(
                table: "Parameters",
                keyColumn: "Id",
                keyValue: 1,
                column: "Value",
                value: "{\n  \"defaultChannel\": \"Email\"\n}");
        }
    }
}
