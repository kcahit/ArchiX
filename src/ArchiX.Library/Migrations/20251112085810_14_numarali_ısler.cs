using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class _14_numarali_ısler : Migration
    {
        // Reuse arrays to avoid per-call allocations (CA1861) and simplify collections (IDE0300)
        private static readonly string[] Columns_Cidr_IsActive = ["Cidr", "IsActive"];
        private static readonly string[] Columns_ServerName_IsActive = ["ServerName", "IsActive"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArchiXSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Group = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsProtected = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiXSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConnectionAudit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: false),
                    NormalizedServer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Result = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ReasonCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    RawConnectionMasked = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionAudit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConnectionServerWhitelist",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Cidr = table.Column<string>(type: "nvarchar(43)", maxLength: 43, nullable: true),
                    Port = table.Column<int>(type: "int", nullable: true),
                    EnvScope = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AddedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionServerWhitelist", x => x.Id);
                    table.CheckConstraint("CK_Whitelist_ServerOrCidr", "[ServerName] IS NOT NULL OR [Cidr] IS NOT NULL");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiXSettings_Group",
                table: "ArchiXSettings",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiXSettings_Key",
                table: "ArchiXSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionAudit_AttemptedAt",
                table: "ConnectionAudit",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionAudit_CorrelationId",
                table: "ConnectionAudit",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionAudit_Result",
                table: "ConnectionAudit",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionServerWhitelist_Cidr_IsActive",
                table: "ConnectionServerWhitelist",
                columns: Columns_Cidr_IsActive);

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionServerWhitelist_EnvScope",
                table: "ConnectionServerWhitelist",
                column: "EnvScope");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionServerWhitelist_ServerName_IsActive",
                table: "ConnectionServerWhitelist",
                columns: Columns_ServerName_IsActive);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchiXSettings");

            migrationBuilder.DropTable(
                name: "ConnectionAudit");

            migrationBuilder.DropTable(
                name: "ConnectionServerWhitelist");
        }
    }
}
