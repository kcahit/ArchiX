using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class ParameterAddId3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Parameters",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.InsertData(
                table: "Parameters",
                columns: new[] { "Id", "ApplicationId", "CreatedBy", "Description", "Group", "IsProtected", "Key", "LastStatusBy", "ParameterDataTypeId", "StatusId", "Template", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[] { 3, 1, 0, "#42 TabbedOptions JSON. navigationMode=Mod(Tabbed/FullPage); tabbed.maxOpenTabs=Maks tab; tabbed.tabAutoCloseMinutes=Oto kapanış(dk); tabbed.autoCloseWarningSeconds=Uyarı(sn); tabbed.enableNestedTabs=Nested tab; tabbed.requireTabContext=Direct link engeli.", "UI", false, "TabbedOptions", 0, 15, 3, "{\n  \"version\": 1,\n  \"navigationMode\": \"Tabbed\",\n  \"tabbed\": {\n    \"maxOpenTabs\": 15,\n    \"onMaxTabReached\": {\n      \"behavior\": \"Block\",\n      \"message\": \"Açık tab sayısı 15 limitine geldi. Lütfen açık tablardan birini kapatınız.\"\n    },\n    \"enableNestedTabs\": false,\n    \"requireTabContext\": true,\n    \"tabAutoCloseMinutes\": 10,\n    \"autoCloseWarningSeconds\": 30,\n    \"tabTitleUniqueSuffix\": { \"format\": \"_{000}\", \"start\": 1 }\n  },\n  \"fullPage\": {\n    \"defaultLandingRoute\": \"/Dashboard\",\n    \"openReportsInNewWindow\": false,\n    \"confirmOnUnsavedChanges\": true,\n    \"deepLinkEnabled\": true,\n    \"errorMode\": \"DefaultErrorPage\",\n    \"enableKeepAlive\": true,\n    \"sessionTimeoutWarningSeconds\": 60\n  }\n}", null, null, "{\n  \"version\": 1,\n  \"navigationMode\": \"Tabbed\",\n  \"tabbed\": {\n    \"maxOpenTabs\": 15,\n    \"onMaxTabReached\": {\n      \"behavior\": \"Block\",\n      \"message\": \"Açık tab sayısı 15 limitine geldi. Lütfen açık tablardan birini kapatınız.\"\n    },\n    \"enableNestedTabs\": true,\n    \"requireTabContext\": true,\n    \"tabAutoCloseMinutes\": 10,\n    \"autoCloseWarningSeconds\": 30,\n    \"tabTitleUniqueSuffix\": { \"format\": \"_{000}\", \"start\": 1 }\n  },\n  \"fullPage\": {\n    \"defaultLandingRoute\": \"/Dashboard\",\n    \"openReportsInNewWindow\": false,\n    \"confirmOnUnsavedChanges\": true,\n    \"deepLinkEnabled\": true,\n    \"errorMode\": \"DefaultErrorPage\",\n    \"enableKeepAlive\": true,\n    \"sessionTimeoutWarningSeconds\": 60\n  }\n}" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Parameters",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Parameters",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);
        }
    }
}
