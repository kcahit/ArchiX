using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class InitCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Status",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    LastStatusAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: true, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    LastStatusBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Status", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FilterItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    LastStatusAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: true, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    LastStatusBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilterItems_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LanguagePacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FieldName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Culture = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    LastStatusAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: true, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    LastStatusBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguagePacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LanguagePacks_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FilterItems_ItemType_Code",
                table: "FilterItems",
                columns: new[] { "ItemType", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FilterItems_StatusId",
                table: "FilterItems",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_LanguagePacks_ItemType_EntityName_FieldName_Code_Culture",
                table: "LanguagePacks",
                columns: new[] { "ItemType", "EntityName", "FieldName", "Code", "Culture" },
                unique: true,
                filter: "[EntityName] IS NOT NULL AND [FieldName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LanguagePacks_StatusId",
                table: "LanguagePacks",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Status_Code",
                table: "Status",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Status_StatusId",
                table: "Status",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilterItems");

            migrationBuilder.DropTable(
                name: "LanguagePacks");

            migrationBuilder.DropTable(
                name: "Status");
        }
    }
}
