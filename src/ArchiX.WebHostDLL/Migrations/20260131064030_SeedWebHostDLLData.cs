using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ArchiX.WebHostDLL.Migrations
{
    /// <inheritdoc />
    public partial class SeedWebHostDLLData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Applications",
                columns: new[] { "Id", "Code", "ConfigVersion", "CreatedAt", "CreatedBy", "DefaultCulture", "Description", "ExternalKey", "IsProtected", "LastStatusAt", "LastStatusBy", "Name", "RowId", "StatusId", "TimeZoneId", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 2, "WebHostDLL", 1, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, "tr-TR", "WebHostDLL customer application", null, false, null, 0, "WebHostDLL", new Guid("00000000-0000-0000-0000-000000000000"), 3, "Europe/Istanbul", null, null });

            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "Id", "ApplicationId", "CreatedAt", "CreatedBy", "Icon", "IsProtected", "LastStatusAt", "LastStatusBy", "ParentId", "RowId", "SortOrder", "StatusId", "Title", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[,]
                {
                    { 1, 2, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, false, null, 0, null, new Guid("00000000-0000-0000-0000-000000000000"), 1, 3, "Dashboard", null, null, "/Dashboard" },
                    { 2, 2, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, false, null, 0, null, new Guid("00000000-0000-0000-0000-000000000000"), 2, 3, "Tanımlar", null, null, "/Definitions" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
