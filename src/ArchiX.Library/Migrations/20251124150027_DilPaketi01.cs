using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class DilPaketi01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
