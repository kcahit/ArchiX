using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable


namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class ParameterSchemaRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parameters_Applications_ApplicationId",
                table: "Parameters");

            migrationBuilder.DropIndex(
                name: "IX_Parameters_ApplicationId",
                table: "Parameters");

            migrationBuilder.DropIndex(
                name: "IX_Parameters_Group_Key_ApplicationId",
                table: "Parameters");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "Parameters");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Parameters");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Parameters");

            migrationBuilder.AlterColumn<string>(
                name: "Template",
                table: "Parameters",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<int>(
                name: "ParameterDataTypeId",
                table: "Parameters",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Parameters",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000)
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.CreateTable(
                name: "ParameterApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParameterId = table.Column<int>(type: "int", nullable: false),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("PK_ParameterApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParameterApplications_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParameterApplications_Parameters_ParameterId",
                        column: x => x.ParameterId,
                        principalTable: "Parameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParameterApplications_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ParameterApplications",
                columns: new[] { "Id", "ApplicationId", "CreatedAt", "CreatedBy", "IsProtected", "LastStatusBy", "ParameterId", "RowId", "StatusId", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { 1, 1, new DateTimeOffset(new DateTime(2025, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, true, 0, 1, new Guid("10000000-0000-0000-0000-000000000001"), 3, null, null, "{\n  \"defaultChannel\": \"Email\",\n  \"channels\": {\n    \"Sms\": { \"codeLength\": 6, \"expirySeconds\": 300 },\n    \"Email\": { \"codeLength\": 6, \"expirySeconds\": 300 },\n    \"Authenticator\": { \"digits\": 6, \"periodSeconds\": 30, \"hashAlgorithm\": \"SHA1\" }\n  }\n}" },
                    { 2, 1, new DateTimeOffset(new DateTime(2025, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, true, 0, 2, new Guid("10000000-0000-0000-0000-000000000002"), 3, null, null, "{\n  \"version\": 1,\n  \"minLength\": 12,\n  \"maxLength\": 128,\n  \"requireUpper\": true,\n  \"requireLower\": true,\n  \"requireDigit\": true,\n  \"requireSymbol\": true,\n  \"allowedSymbols\": \"!@#$%^&*_-+=:?.,;\",\n  \"minDistinctChars\": 5,\n  \"maxRepeatedSequence\": 3,\n  \"blockList\": [\"password\", \"123456\", \"qwerty\", \"admin\"],\n  \"historyCount\": 10,\n  \"maxPasswordAgeDays\": null,\n  \"lockoutThreshold\": 5,\n  \"lockoutSeconds\": 900,\n  \"hash\": {\n    \"algorithm\": \"Argon2id\",\n    \"memoryKb\": 65536,\n    \"parallelism\": 2,\n    \"iterations\": 3,\n    \"saltLength\": 16,\n    \"hashLength\": 32,\n    \"fallback\": { \"algorithm\": \"PBKDF2-SHA512\", \"iterations\": 210000 },\n    \"pepperEnabled\": true\n  }\n}" },
                    { 3, 1, new DateTimeOffset(new DateTime(2025, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, true, 0, 3, new Guid("10000000-0000-0000-0000-000000000003"), 3, null, null, "{\n  \"version\": 1,\n  \"navigationMode\": \"Tabbed\",\n  \"tabbed\": {\n    \"maxOpenTabs\": 15,\n    \"onMaxTabReached\": {\n      \"behavior\": \"Block\",\n      \"message\": \"Açık tab sayısı 15 limitine geldi. Lütfen açık tablardan birini kapatınız.\"\n    },\n    \"enableNestedTabs\": true,\n    \"requireTabContext\": true,\n    \"tabAutoCloseMinutes\": 10,\n    \"autoCloseWarningSeconds\": 30,\n    \"tabTitleUniqueSuffix\": { \"format\": \"_{000}\", \"start\": 1 }\n  },\n  \"fullPage\": {\n    \"defaultLandingRoute\": \"/Dashboard\",\n    \"openReportsInNewWindow\": false,\n    \"confirmOnUnsavedChanges\": true,\n    \"deepLinkEnabled\": true,\n    \"errorMode\": \"DefaultErrorPage\",\n    \"enableKeepAlive\": true,\n    \"sessionTimeoutWarningSeconds\": 60\n  }\n}" }
                });

            migrationBuilder.UpdateData(
                table: "Parameters",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "IsProtected", "RowId" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, new Guid("00000000-0000-0000-0000-000000000001") });

            migrationBuilder.UpdateData(
                table: "Parameters",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "IsProtected", "RowId" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, new Guid("00000000-0000-0000-0000-000000000002") });

            migrationBuilder.UpdateData(
                table: "Parameters",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "IsProtected", "RowId" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, new Guid("00000000-0000-0000-0000-000000000003") });

            migrationBuilder.InsertData(
                table: "Parameters",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "Group", "IsProtected", "Key", "LastStatusBy", "ParameterDataTypeId", "RowId", "StatusId", "Template", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 4, new DateTimeOffset(new DateTime(2025, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, "#57 UI timeout parametreleri (session, warning, tab request timeout)", "UI", true, "TimeoutOptions", 0, 15, new Guid("00000000-0000-0000-0000-000000000004"), 3, "{\"sessionTimeoutSeconds\":645,\"sessionWarningSeconds\":45,\"tabRequestTimeoutMs\":30000}", null, null },
                    { 5, new DateTimeOffset(new DateTime(2025, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, "#57 HTTP retry ve timeout politikaları", "HTTP", true, "HttpPoliciesOptions", 0, 15, new Guid("00000000-0000-0000-0000-000000000005"), 3, "{\"retryCount\":2,\"baseDelayMs\":200,\"timeoutSeconds\":30}", null, null },
                    { 6, new DateTimeOffset(new DateTime(2025, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, "#57 Güvenlik attempt limiter parametreleri", "Security", true, "AttemptLimiterOptions", 0, 15, new Guid("00000000-0000-0000-0000-000000000006"), 3, "{\"window\":600,\"maxAttempts\":5,\"cooldownSeconds\":300}", null, null },
                    { 7, new DateTimeOffset(new DateTime(2025, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, "#57 Parametre cache TTL süreleri", "System", true, "ParameterRefresh", 0, 15, new Guid("00000000-0000-0000-0000-000000000007"), 3, "{\"uiCacheTtlSeconds\":300,\"httpCacheTtlSeconds\":60,\"securityCacheTtlSeconds\":30}", null, null }
                });

            migrationBuilder.InsertData(
                table: "ParameterApplications",
                columns: new[] { "Id", "ApplicationId", "CreatedAt", "CreatedBy", "IsProtected", "LastStatusBy", "ParameterId", "RowId", "StatusId", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { 4, 1, new DateTimeOffset(new DateTime(2025, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, true, 0, 4, new Guid("10000000-0000-0000-0000-000000000004"), 3, null, null, "{\"sessionTimeoutSeconds\":645,\"sessionWarningSeconds\":45,\"tabRequestTimeoutMs\":30000}" },
                    { 5, 1, new DateTimeOffset(new DateTime(2025, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, true, 0, 5, new Guid("10000000-0000-0000-0000-000000000005"), 3, null, null, "{\"retryCount\":2,\"baseDelayMs\":200,\"timeoutSeconds\":30}" },
                    { 6, 1, new DateTimeOffset(new DateTime(2025, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, true, 0, 6, new Guid("10000000-0000-0000-0000-000000000006"), 3, null, null, "{\"window\":600,\"maxAttempts\":5,\"cooldownSeconds\":300}" },
                    { 7, 1, new DateTimeOffset(new DateTime(2025, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, true, 0, 7, new Guid("10000000-0000-0000-0000-000000000007"), 3, null, null, "{\"uiCacheTtlSeconds\":300,\"httpCacheTtlSeconds\":60,\"securityCacheTtlSeconds\":30}" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parameters_Group_Key",
                table: "Parameters",
                columns: new[] { "Group", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParameterApplications_ApplicationId",
                table: "ParameterApplications",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterApplications_ParameterId_ApplicationId",
                table: "ParameterApplications",
                columns: new[] { "ParameterId", "ApplicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParameterApplications_StatusId",
                table: "ParameterApplications",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParameterApplications");

            migrationBuilder.DropIndex(
                name: "IX_Parameters_Group_Key",
                table: "Parameters");

            migrationBuilder.DeleteData(
                table: "Parameters",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Parameters",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Parameters",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Parameters",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.AlterColumn<string>(
                name: "Template",
                table: "Parameters",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<int>(
                name: "ParameterDataTypeId",
                table: "Parameters",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Parameters",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AddColumn<int>(
                name: "ApplicationId",
                table: "Parameters",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Parameters",
                type: "rowversion",
                rowVersion: true,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "Parameters",
                type: "nvarchar(max)",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.UpdateData(
                table: "Parameters",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApplicationId", "CreatedAt", "IsProtected", "RowId", "Value" },
                values: new object[] { 1, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("00000000-0000-0000-0000-000000000000"), "{\n  \"defaultChannel\": \"Email\",\n  \"channels\": {\n    \"Sms\": { \"codeLength\": 6, \"expirySeconds\": 300 },\n    \"Email\": { \"codeLength\": 6, \"expirySeconds\": 300 },\n    \"Authenticator\": { \"digits\": 6, \"periodSeconds\": 30, \"hashAlgorithm\": \"SHA1\" }\n  }\n}" });

            migrationBuilder.UpdateData(
                table: "Parameters",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApplicationId", "CreatedAt", "IsProtected", "RowId", "Value" },
                values: new object[] { 1, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("00000000-0000-0000-0000-000000000000"), "{\n  \"version\": 1,\n  \"minLength\": 12,\n  \"maxLength\": 128,\n  \"requireUpper\": true,\n  \"requireLower\": true,\n  \"requireDigit\": true,\n  \"requireSymbol\": true,\n  \"allowedSymbols\": \"!@#$%^&*_-+=:?.,;\",\n  \"minDistinctChars\": 5,\n  \"maxRepeatedSequence\": 3,\n  \"blockList\": [\"password\", \"123456\", \"qwerty\", \"admin\"],\n  \"historyCount\": 10,\n  \"maxPasswordAgeDays\": null,\n  \"lockoutThreshold\": 5,\n  \"lockoutSeconds\": 900,\n  \"hash\": {\n    \"algorithm\": \"Argon2id\",\n    \"memoryKb\": 65536,\n    \"parallelism\": 2,\n    \"iterations\": 3,\n    \"saltLength\": 16,\n    \"hashLength\": 32,\n    \"fallback\": { \"algorithm\": \"PBKDF2-SHA512\", \"iterations\": 210000 },\n    \"pepperEnabled\": true\n  }\n}" });

            migrationBuilder.UpdateData(
                table: "Parameters",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApplicationId", "CreatedAt", "IsProtected", "RowId", "Value" },
                values: new object[] { 1, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("00000000-0000-0000-0000-000000000000"), "{\n  \"version\": 1,\n  \"navigationMode\": \"Tabbed\",\n  \"tabbed\": {\n    \"maxOpenTabs\": 15,\n    \"onMaxTabReached\": {\n      \"behavior\": \"Block\",\n      \"message\": \"Açık tab sayısı 15 limitine geldi. Lütfen açık tablardan birini kapatınız.\"\n    },\n    \"enableNestedTabs\": true,\n    \"requireTabContext\": true,\n    \"tabAutoCloseMinutes\": 10,\n    \"autoCloseWarningSeconds\": 30,\n    \"tabTitleUniqueSuffix\": { \"format\": \"_{000}\", \"start\": 1 }\n  },\n  \"fullPage\": {\n    \"defaultLandingRoute\": \"/Dashboard\",\n    \"openReportsInNewWindow\": false,\n    \"confirmOnUnsavedChanges\": true,\n    \"deepLinkEnabled\": true,\n    \"errorMode\": \"DefaultErrorPage\",\n    \"enableKeepAlive\": true,\n    \"sessionTimeoutWarningSeconds\": 60\n  }\n}" });

            migrationBuilder.CreateIndex(
                name: "IX_Parameters_ApplicationId",
                table: "Parameters",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Parameters_Group_Key_ApplicationId",
                table: "Parameters",
                columns: new[] { "Group", "Key", "ApplicationId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Parameters_Applications_ApplicationId",
                table: "Parameters",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
