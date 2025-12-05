using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ArchiX.Library.src.ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationParameter_Refactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Parameters_Group_Key",
                table: "Parameters");

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "LanguagePacks",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.AddColumn<int>(
                name: "ApplicationId",
                table: "Parameters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DefaultCulture = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TimeZoneId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConfigVersion = table.Column<int>(type: "int", nullable: false),
                    ExternalKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Applications",
                columns: new[] { "Id", "Code", "ConfigVersion", "CreatedBy", "DefaultCulture", "Description", "ExternalKey", "IsProtected", "LastStatusBy", "Name", "StatusId", "TimeZoneId", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 1, "Global", 1, 0, "tr-TR", "Default/global scope", null, false, 0, "Global Application", 3, null, null, null });

            migrationBuilder.UpdateData(
                table: "Parameters",
                keyColumn: "Id",
                keyValue: 1,
                column: "ApplicationId",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Parameters_ApplicationId",
                table: "Parameters",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Parameters_Group_Key_ApplicationId",
                table: "Parameters",
                columns: new[] { "Group", "Key", "ApplicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Code",
                table: "Applications",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_StatusId",
                table: "Applications",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parameters_Applications_ApplicationId",
                table: "Parameters",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parameters_Applications_ApplicationId",
                table: "Parameters");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Parameters_ApplicationId",
                table: "Parameters");

            migrationBuilder.DropIndex(
                name: "IX_Parameters_Group_Key_ApplicationId",
                table: "Parameters");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "Parameters");

            migrationBuilder.InsertData(
                table: "LanguagePacks",
                columns: new[] { "Id", "Code", "CreatedBy", "Culture", "Description", "DisplayName", "EntityName", "FieldName", "IsProtected", "ItemType", "LastStatusBy", "StatusId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 5, "StartsWith", 0, "tr-TR", "Değer belirtilen ifadeyle başlamalı", "Başlar", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 6, "StartsWith", 0, "en-US", "Value must start with the given text", "Starts With", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 7, "NotStartsWith", 0, "tr-TR", "Değer belirtilen ifadeyle başlamamalı", "Başlamaz", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 8, "NotStartsWith", 0, "en-US", "Value must not start with the given text", "Does Not Start With", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 9, "EndsWith", 0, "tr-TR", "Değer belirtilen ifadeyle bitmeli", "Biter", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 10, "EndsWith", 0, "en-US", "Value must end with the given text", "Ends With", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 11, "NotEndsWith", 0, "tr-TR", "Değer belirtilen ifadeyle bitmemeli", "Bitmez", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 12, "NotEndsWith", 0, "en-US", "Value must not end with the given text", "Does Not End With", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 13, "Contains", 0, "tr-TR", "Değer belirtilen ifadeyi içermeli", "İçerir", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 14, "Contains", 0, "en-US", "Value must contain the given text", "Contains", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 15, "NotContains", 0, "tr-TR", "Değer belirtilen ifadeyi içermemeli", "İçermez", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 16, "NotContains", 0, "en-US", "Value must not contain the given text", "Does Not Contain", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 17, "Between", 0, "tr-TR", "Değer alt ve üst sınırlar arasında (dahil) olmalı", "Arasında", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 18, "Between", 0, "en-US", "Value must be between lower and upper bounds (inclusive)", "Between", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 19, "NotBetween", 0, "tr-TR", "Değer alt ve üst sınırlar arasında olmamalı", "Arasında Değil", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 20, "NotBetween", 0, "en-US", "Value must not be between the given bounds", "Not Between", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 21, "GreaterThan", 0, "tr-TR", "Değer belirtilenden büyük olmalı", "Büyüktür", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 22, "GreaterThan", 0, "en-US", "Value must be greater than the given one", "Greater Than", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 23, "GreaterThanOrEqual", 0, "tr-TR", "Değer belirtilenden büyük veya eşit olmalı", "Büyük veya Eşittir", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 24, "GreaterThanOrEqual", 0, "en-US", "Value must be greater than or equal to the given one", "Greater Than Or Equal", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 25, "LessThan", 0, "tr-TR", "Değer belirtilenden küçük olmalı", "Küçüktür", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 26, "LessThan", 0, "en-US", "Value must be less than the given one", "Less Than", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 27, "LessThanOrEqual", 0, "tr-TR", "Değer belirtilenden küçük veya eşit olmalı", "Küçük veya Eşittir", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 28, "LessThanOrEqual", 0, "en-US", "Value must be less than or equal to the given one", "Less Than Or Equal", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 29, "In", 0, "tr-TR", "Değer verilen listedeki öğelerden biri olmalı", "İçinde", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 30, "In", 0, "en-US", "Value must be one of the provided list items", "In Set", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 31, "NotIn", 0, "tr-TR", "Değer verilen listedeki öğelerden biri olmamalı", "İçinde Değil", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 32, "NotIn", 0, "en-US", "Value must not be any of the provided list items", "Not In Set", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 33, "IsNull", 0, "tr-TR", "Değer null olmalı", "Boş (Null)", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 34, "IsNull", 0, "en-US", "Value must be null", "Is Null", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 35, "IsNotNull", 0, "tr-TR", "Değer null olmamalı", "Boş Değil", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 36, "IsNotNull", 0, "en-US", "Value must not be null", "Is Not Null", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 37, "DFT", 0, "tr-TR", "Kayıt taslak durumunda", "Taslak", "Statu", "Code", false, "Status", 0, 3, null, null },
                    { 38, "DFT", 0, "en-US", "Record is in draft state", "Draft", "Statu", "Code", false, "Status", 0, 3, null, null },
                    { 39, "AWT", 0, "tr-TR", "Kayıt onay bekliyor", "Onay Bekliyor", "Statu", "Code", false, "Status", 0, 3, null, null },
                    { 40, "AWT", 0, "en-US", "Record is waiting for approval", "Awaiting Approval", "Statu", "Code", false, "Status", 0, 3, null, null },
                    { 41, "APR", 0, "tr-TR", "Kayıt onaylandı", "Onaylandı", "Statu", "Code", false, "Status", 0, 3, null, null },
                    { 42, "APR", 0, "en-US", "Record has been approved", "Approved", "Statu", "Code", false, "Status", 0, 3, null, null },
                    { 43, "REJ", 0, "tr-TR", "Kayıt reddedildi", "Reddedildi", "Statu", "Code", false, "Status", 0, 3, null, null },
                    { 44, "REJ", 0, "en-US", "Record has been rejected", "Rejected", "Statu", "Code", false, "Status", 0, 3, null, null },
                    { 45, "PSV", 0, "tr-TR", "Kayıt pasif / devre dışı", "Pasif", "Statu", "Code", false, "Status", 0, 3, null, null },
                    { 46, "PSV", 0, "en-US", "Record is passive / inactive", "Passive", "Statu", "Code", false, "Status", 0, 3, null, null },
                    { 47, "DEL", 0, "tr-TR", "Kayıt silinmiş durumda", "Silindi", "Statu", "Code", false, "Status", 0, 3, null, null },
                    { 48, "DEL", 0, "en-US", "Record has been deleted", "Deleted", "Statu", "Code", false, "Status", 0, 3, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parameters_Group_Key",
                table: "Parameters",
                columns: new[] { "Group", "Key" },
                unique: true);
        }
    }
}
