using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class MigDilPaketiFiltre01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                });

            migrationBuilder.CreateTable(
                name: "Status",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
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

            migrationBuilder.InsertData(
                table: "FilterItems",
                columns: new[] { "Id", "Code", "CreatedBy", "ItemType", "LastStatusBy", "StatusId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { -27, "IsNotNull", 0, "Operator", 0, 3, null, null },
                    { -26, "IsNull", 0, "Operator", 0, 3, null, null },
                    { -25, "NotIn", 0, "Operator", 0, 3, null, null },
                    { -24, "In", 0, "Operator", 0, 3, null, null },
                    { -23, "LessThanOrEqual", 0, "Operator", 0, 3, null, null },
                    { -22, "LessThan", 0, "Operator", 0, 3, null, null },
                    { -21, "GreaterThanOrEqual", 0, "Operator", 0, 3, null, null },
                    { -20, "GreaterThan", 0, "Operator", 0, 3, null, null },
                    { -19, "NotBetween", 0, "Operator", 0, 3, null, null },
                    { -18, "Between", 0, "Operator", 0, 3, null, null },
                    { -17, "NotContains", 0, "Operator", 0, 3, null, null },
                    { -16, "Contains", 0, "Operator", 0, 3, null, null },
                    { -15, "NotEndsWith", 0, "Operator", 0, 3, null, null },
                    { -14, "EndsWith", 0, "Operator", 0, 3, null, null },
                    { -13, "NotStartsWith", 0, "Operator", 0, 3, null, null },
                    { -12, "StartsWith", 0, "Operator", 0, 3, null, null },
                    { -11, "NotEquals", 0, "Operator", 0, 3, null, null },
                    { -10, "Equals", 0, "Operator", 0, 3, null, null }
                });

            migrationBuilder.InsertData(
                table: "LanguagePacks",
                columns: new[] { "Id", "Code", "CreatedBy", "Culture", "Description", "DisplayName", "EntityName", "FieldName", "ItemType", "LastStatusBy", "StatusId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { -1036, "IsNotNull", 0, "en-US", "Value is not null and not empty", "Is Not Null/Empty", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1035, "IsNotNull", 0, "tr-TR", "Değer null değil ve empty değil ise", "Boş Değil", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1034, "IsNull", 0, "en-US", "Value is null or empty", "Is Null/Empty", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1033, "IsNull", 0, "tr-TR", "Değer null veya empty ise", "Boş", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1032, "NotIn", 0, "en-US", "Value must not be in the given list", "Not In", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1031, "NotIn", 0, "tr-TR", "Liste içindeki değerlerden biri değilse", "İçinde Değil", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1030, "In", 0, "en-US", "Value must be in the given list", "In", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1029, "In", 0, "tr-TR", "Liste içindeki değerlerden biriyse", "İçinde", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1028, "LessThanOrEqual", 0, "en-US", "Value must be less than or equal to given value", "Less Than Or Equal", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1027, "LessThanOrEqual", 0, "tr-TR", "Belirtilen değerden küçük ya da eşit", "Küçük veya Eşit", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1026, "LessThan", 0, "en-US", "Value must be less than given value", "Less Than", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1025, "LessThan", 0, "tr-TR", "Belirtilen değerden küçükse", "Küçük", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1024, "GreaterThanOrEqual", 0, "en-US", "Value must be greater than or equal to given value", "Greater Than Or Equal", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1023, "GreaterThanOrEqual", 0, "tr-TR", "Belirtilen değerden büyük ya da eşit", "Büyük veya Eşit", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1022, "GreaterThan", 0, "en-US", "Value must be greater than given value", "Greater Than", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1021, "GreaterThan", 0, "tr-TR", "Belirtilen değerden büyükse", "Büyük", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1020, "NotBetween", 0, "en-US", "Value must not be between two values", "Not Between", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1019, "NotBetween", 0, "tr-TR", "İki değer arasında olmamalı", "Arasında Değil", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1018, "Between", 0, "en-US", "Value must be between two values", "Between", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1017, "Between", 0, "tr-TR", "İki değer arasındaysa", "Arasında", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1016, "NotContains", 0, "en-US", "Value must not contain given text", "Does Not Contain", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1015, "NotContains", 0, "tr-TR", "İçinde geçen değer yoksa", "İçermez", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1014, "Contains", 0, "en-US", "Value contains given text", "Contains", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1013, "Contains", 0, "tr-TR", "İçinde geçen değer varsa", "İçerir", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1012, "NotEndsWith", 0, "en-US", "Value must not end with given text", "Does Not End With", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1011, "NotEndsWith", 0, "tr-TR", "Bitiş eşleşmesi değil", "Bitmez", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1010, "EndsWith", 0, "en-US", "Value ends with given text", "Ends With", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1009, "EndsWith", 0, "tr-TR", "Bitiş eşleşmesi", "Biter", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1008, "NotStartsWith", 0, "en-US", "Value must not start with given text", "Does Not Start With", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1007, "NotStartsWith", 0, "tr-TR", "Başlangıç eşleşmesi değil", "Başlamaz", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1006, "StartsWith", 0, "en-US", "Value starts with given text", "Starts With", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1005, "StartsWith", 0, "tr-TR", "Başlangıç eşleşmesi", "Başlar", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1004, "NotEquals", 0, "en-US", "Value must not be equal", "Not Equal", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1003, "NotEquals", 0, "tr-TR", "Değer eşit olmamalı", "Eşit Değil", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1002, "Equals", 0, "en-US", "Value must be equal", "Equals", "FilterItem", "Code", "Operator", 0, 3, null, null },
                    { -1001, "Equals", 0, "tr-TR", "Değer eşit olmalı", "Eşittir", "FilterItem", "Code", "Operator", 0, 3, null, null }
                });

            migrationBuilder.InsertData(
                table: "Status",
                columns: new[] { "Id", "Code", "CreatedBy", "Description", "LastStatusBy", "Name", "StatusId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { -14, "DEL", 0, "Record has been deleted", 0, "Deleted", 3, null, null },
                    { -13, "PSV", 0, "Record is passive / inactive", 0, "Passive", 3, null, null },
                    { -12, "REJ", 0, "Record has been rejected", 0, "Rejected", 3, null, null },
                    { -11, "APR", 0, "Record has been approved", 0, "Approved", 3, null, null },
                    { -10, "AWT", 0, "Record is waiting for approval", 0, "Awaiting Approval", 3, null, null },
                    { -1, "DFT", 0, "Record is in draft state", 0, "Draft", 3, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FilterItems_ItemType_Code",
                table: "FilterItems",
                columns: new[] { "ItemType", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LanguagePacks_ItemType_EntityName_FieldName_Code_Culture",
                table: "LanguagePacks",
                columns: new[] { "ItemType", "EntityName", "FieldName", "Code", "Culture" },
                unique: true,
                filter: "[EntityName] IS NOT NULL AND [FieldName] IS NOT NULL");
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
