using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordAgingToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Users tablosuna PasswordChangedAtUtc ekle
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PasswordChangedAtUtc",
                table: "Users",
                type: "datetimeoffset(4)",
                precision: 4,
                nullable: true);

            // 2. Users tablosuna MaxPasswordAgeDays ekle
            migrationBuilder.AddColumn<int>(
                name: "MaxPasswordAgeDays",
                table: "Users",
                type: "int",
                nullable: true);

            ////// 3. ✅ Admin kullanıcısı için default değerleri set et
            ////migrationBuilder.Sql(@"
            ////    UPDATE Users 
            ////    SET PasswordChangedAtUtc = SYSDATETIMEOFFSET(),
            ////        MaxPasswordAgeDays = 90
            ////    WHERE Id = 1 AND PasswordChangedAtUtc IS NULL;
            ////");

            // 4. Index ekle (performans için)
            migrationBuilder.CreateIndex(
                name: "IX_Users_PasswordChangedAtUtc",
                table: "Users",
                column: "PasswordChangedAtUtc");

            // 5. Parameters seed data güncelle
            migrationBuilder.UpdateData(
                table: "Parameters",
                keyColumn: "Id",
                keyValue: 2,
                column: "Value",
                value: "{\n  \"version\": 1,\n  \"minLength\": 12,\n  \"maxLength\": 128,\n  \"requireUpper\": true,\n  \"requireLower\": true,\n  \"requireDigit\": true,\n  \"requireSymbol\": true,\n  \"allowedSymbols\": \"!@#$%^&*_-+=:?.,;\",\n  \"minDistinctChars\": 5,\n  \"maxRepeatedSequence\": 3,\n  \"blockList\": [\"password\", \"123456\", \"qwerty\", \"admin\"],\n  \"historyCount\": 10,\n  \"maxPasswordAgeDays\": null,\n  \"lockoutThreshold\": 5,\n  \"lockoutSeconds\": 900,\n  \"hash\": {\n    \"algorithm\": \"Argon2id\",\n    \"memoryKb\": 65536,\n    \"parallelism\": 2,\n    \"iterations\": 3,\n    \"saltLength\": 16,\n    \"hashLength\": 32,\n    \"fallback\": { \"algorithm\": \"PBKDF2-SHA512\", \"iterations\": 210000 },\n    \"pepperEnabled\": true\n  }\n}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alma (ters sıra)
            migrationBuilder.UpdateData(
                table: "Parameters",
                keyColumn: "Id",
                keyValue: 2,
                column: "Value",
                value: "{\n  \"version\": 1,\n  \"minLength\": 12,\n  \"maxLength\": 128,\n  \"requireUpper\": true,\n  \"requireLower\": true,\n  \"requireDigit\": true,\n  \"requireSymbol\": true,\n  \"allowedSymbols\": \"!@#$%^&*_-+=:?.,;\",\n  \"minDistinctChars\": 5,\n  \"maxRepeatedSequence\": 3,\n  \"blockList\": [\"password\", \"123456\", \"qwerty\", \"admin\"],\n  \"historyCount\": 10,\n  \"lockoutThreshold\": 5,\n  \"lockoutSeconds\": 900,\n  \"hash\": {\n    \"algorithm\": \"Argon2id\",\n    \"memoryKb\": 65536,\n    \"parallelism\": 2,\n    \"iterations\": 3,\n    \"saltLength\": 16,\n    \"hashLength\": 32,\n    \"fallback\": { \"algorithm\": \"PBKDF2-SHA512\", \"iterations\": 210000 },\n    \"pepperEnabled\": true\n  }\n}");

            migrationBuilder.DropIndex(
                name: "IX_Users_PasswordChangedAtUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MaxPasswordAgeDays",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordChangedAtUtc",
                table: "Users");
        }
    }
}
