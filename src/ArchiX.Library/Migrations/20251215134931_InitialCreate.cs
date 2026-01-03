using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    LastStatusBy = table.Column<int>(type: "int", nullable: false),
                    IsProtected = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Status", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "ConnectionAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: false),
                    NormalizedServer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Result = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ReasonCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RawConnectionMasked = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    RowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    LastStatusAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: true, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    LastStatusBy = table.Column<int>(type: "int", nullable: false),
                    IsProtected = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConnectionAudits_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConnectionServerWhitelists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Cidr = table.Column<string>(type: "nvarchar(43)", maxLength: 43, nullable: true),
                    EnvScope = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    LastStatusAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: true, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    LastStatusBy = table.Column<int>(type: "int", nullable: false),
                    IsProtected = table.Column<bool>(type: "bit", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionServerWhitelists", x => x.Id);
                    table.CheckConstraint("CK_Whitelist_ServerOrCidr", "[ServerName] IS NOT NULL OR [Cidr] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_ConnectionServerWhitelists_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    LastStatusBy = table.Column<int>(type: "int", nullable: false),
                    IsProtected = table.Column<bool>(type: "bit", nullable: false)
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
                    LastStatusBy = table.Column<int>(type: "int", nullable: false),
                    IsProtected = table.Column<bool>(type: "bit", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ParameterDataTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("PK_ParameterDataTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParameterDataTypes_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "UserPasswordHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    HashAlgorithm = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: false),
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
                    table.PrimaryKey("PK_UserPasswordHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPasswordHistories_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    RowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    LastStatusAt = table.Column<DateTimeOffset>(type: "datetimeoffset(4)", precision: 4, nullable: true, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    LastStatusBy = table.Column<int>(type: "int", nullable: false),
                    IsProtected = table.Column<bool>(type: "bit", nullable: false),
                    PasswordChangedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    MaxPasswordAgeDays = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PasswordBlacklists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    Word = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
                    table.PrimaryKey("PK_PasswordBlacklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordBlacklists_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PasswordBlacklists_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Parameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Group = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    ParameterDataTypeId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Template = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                    table.PrimaryKey("PK_Parameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parameters_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Parameters_ParameterDataTypes_ParameterDataTypeId",
                        column: x => x.ParameterDataTypeId,
                        principalTable: "ParameterDataTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Parameters_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_UserApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserApplications_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserApplications_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserApplications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Status",
                columns: new[] { "Id", "Code", "CreatedBy", "Description", "IsProtected", "LastStatusBy", "Name", "StatusId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "DFT", 0, "Record is in draft state", false, 0, "Draft", 3, null, null },
                    { 2, "AWT", 0, "Record is waiting for approval", false, 0, "Awaiting Approval", 3, null, null },
                    { 3, "APR", 0, "Record has been approved", false, 0, "Approved", 3, null, null },
                    { 4, "REJ", 0, "Record has been rejected", false, 0, "Rejected", 3, null, null },
                    { 5, "PSV", 0, "Record is passive / inactive", false, 0, "Passive", 3, null, null },
                    { 6, "DEL", 0, "Record has been deleted", false, 0, "Deleted", 3, null, null }
                });

            migrationBuilder.InsertData(
                table: "Applications",
                columns: new[] { "Id", "Code", "ConfigVersion", "CreatedBy", "DefaultCulture", "Description", "ExternalKey", "IsProtected", "LastStatusBy", "Name", "StatusId", "TimeZoneId", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 1, "Global", 1, 0, "tr-TR", "Default/global scope", null, false, 0, "Global Application", 3, null, null, null });

            migrationBuilder.InsertData(
                table: "FilterItems",
                columns: new[] { "Id", "Code", "CreatedBy", "IsProtected", "ItemType", "LastStatusBy", "StatusId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "Equals", 0, false, "Operator", 0, 3, null, null },
                    { 2, "NotEquals", 0, false, "Operator", 0, 3, null, null },
                    { 3, "StartsWith", 0, false, "Operator", 0, 3, null, null },
                    { 4, "NotStartsWith", 0, false, "Operator", 0, 3, null, null },
                    { 5, "EndsWith", 0, false, "Operator", 0, 3, null, null },
                    { 6, "NotEndsWith", 0, false, "Operator", 0, 3, null, null },
                    { 7, "Contains", 0, false, "Operator", 0, 3, null, null },
                    { 8, "NotContains", 0, false, "Operator", 0, 3, null, null },
                    { 9, "Between", 0, false, "Operator", 0, 3, null, null },
                    { 10, "NotBetween", 0, false, "Operator", 0, 3, null, null },
                    { 11, "GreaterThan", 0, false, "Operator", 0, 3, null, null },
                    { 12, "GreaterThanOrEqual", 0, false, "Operator", 0, 3, null, null },
                    { 13, "LessThan", 0, false, "Operator", 0, 3, null, null },
                    { 14, "LessThanOrEqual", 0, false, "Operator", 0, 3, null, null },
                    { 15, "In", 0, false, "Operator", 0, 3, null, null },
                    { 16, "NotIn", 0, false, "Operator", 0, 3, null, null },
                    { 17, "IsNull", 0, false, "Operator", 0, 3, null, null },
                    { 18, "IsNotNull", 0, false, "Operator", 0, 3, null, null }
                });

            migrationBuilder.InsertData(
                table: "LanguagePacks",
                columns: new[] { "Id", "Code", "CreatedBy", "Culture", "Description", "DisplayName", "EntityName", "FieldName", "IsProtected", "ItemType", "LastStatusBy", "StatusId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "Equals", 0, "tr-TR", "Değer belirtilene eşit olmalı", "Eşittir", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 2, "Equals", 0, "en-US", "Value must be equal to the given one", "Equals", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 3, "NotEquals", 0, "tr-TR", "Değer belirtilene eşit olmamalı", "Eşit Değil", "FilterItem", "Code", false, "Operator", 0, 3, null, null },
                    { 4, "NotEquals", 0, "en-US", "Value must not be equal to the given one", "Not Equal", "FilterItem", "Code", false, "Operator", 0, 3, null, null }
                });

            migrationBuilder.InsertData(
                table: "ParameterDataTypes",
                columns: new[] { "Id", "Category", "Code", "CreatedBy", "Description", "IsProtected", "LastStatusBy", "Name", "StatusId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "NVarChar", 60, 0, "NVARCHAR logical length 50", false, 0, "NVarChar_50", 3, null, null },
                    { 2, "NVarChar", 70, 0, "NVARCHAR logical length 100", false, 0, "NVarChar_100", 3, null, null },
                    { 3, "NVarChar", 80, 0, "NVARCHAR logical length 250", false, 0, "NVarChar_250", 3, null, null },
                    { 4, "NVarChar", 90, 0, "NVARCHAR logical length 500", false, 0, "NVarChar_500", 3, null, null },
                    { 5, "NVarChar", 100, 0, "NVARCHAR(MAX) logical length", false, 0, "NVarChar_Max", 3, null, null },
                    { 6, "Numeric", 200, 0, "Unsigned 8-bit integer", false, 0, "Byte", 3, null, null },
                    { 7, "Numeric", 210, 0, "16-bit integer", false, 0, "SmallInt", 3, null, null },
                    { 8, "Numeric", 220, 0, "32-bit integer", false, 0, "Int", 3, null, null },
                    { 9, "Numeric", 230, 0, "64-bit integer", false, 0, "BigInt", 3, null, null },
                    { 10, "Numeric", 240, 0, "Decimal(18,6)", false, 0, "Decimal18_6", 3, null, null },
                    { 11, "Temporal", 300, 0, "ISO date", false, 0, "Date", 3, null, null },
                    { 12, "Temporal", 310, 0, "ISO time", false, 0, "Time", 3, null, null },
                    { 13, "Temporal", 320, 0, "ISO datetime", false, 0, "DateTime", 3, null, null },
                    { 14, "Other", 900, 0, "Boolean true/false", false, 0, "Bool", 3, null, null },
                    { 15, "Other", 910, 0, "Valid JSON", false, 0, "Json", 3, null, null },
                    { 16, "Other", 920, 0, "Encrypted secret", false, 0, "Secret", 3, null, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedBy", "DisplayName", "Email", "IsAdmin", "IsProtected", "LastStatusBy", "MaxPasswordAgeDays", "NormalizedEmail", "NormalizedUserName", "PasswordChangedAtUtc", "Phone", "StatusId", "UpdatedAt", "UpdatedBy", "UserName" },
                values: new object[] { 1, 0, "System Admin", "admin@example.com", true, true, 0, 90, "ADMIN@EXAMPLE.COM", "ADMIN", null, null, 3, null, null, "admin" });

            migrationBuilder.InsertData(
                table: "Parameters",
                columns: new[] { "Id", "ApplicationId", "CreatedBy", "Description", "Group", "IsProtected", "Key", "LastStatusBy", "ParameterDataTypeId", "StatusId", "Template", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { 1, 1, 0, "İkili doğrulama varsayılan kanal ve seçenekleri", "TwoFactor", false, "Options", 0, 15, 3, "{\n  \"defaultChannel\": \"Sms\",\n  \"channels\": {\n    \"Sms\": { \"codeLength\": 6, \"expirySeconds\": 300 },\n    \"Email\": { \"codeLength\": 6, \"expirySeconds\": 300 },\n    \"Authenticator\": { \"digits\": 6, \"periodSeconds\": 30, \"hashAlgorithm\": \"SHA1\" }\n  }\n}", null, null, "{\n  \"defaultChannel\": \"Email\",\n  \"channels\": {\n    \"Sms\": { \"codeLength\": 6, \"expirySeconds\": 300 },\n    \"Email\": { \"codeLength\": 6, \"expirySeconds\": 300 },\n    \"Authenticator\": { \"digits\": 6, \"periodSeconds\": 30, \"hashAlgorithm\": \"SHA1\" }\n  }\n}" },
                    { 2, 1, 0, "Parola politikası ve hash parametreleri", "Security", false, "PasswordPolicy", 0, 15, 3, "{\n  \"version\": 1,\n  \"minLength\": 12,\n  \"maxLength\": 128,\n  \"requireUpper\": true,\n  \"requireLower\": true,\n  \"requireDigit\": true,\n  \"requireSymbol\": true,\n  \"allowedSymbols\": \"\",\n  \"minDistinctChars\": 0,\n  \"maxRepeatedSequence\": 0,\n  \"blockList\": [],\n  \"historyCount\": 0,\n  \"lockoutThreshold\": 0,\n  \"lockoutSeconds\": 0,\n  \"hash\": {\n    \"algorithm\": \"Argon2id\",\n    \"memoryKb\": 0,\n    \"parallelism\": 0,\n    \"iterations\": 0,\n    \"saltLength\": 0,\n    \"hashLength\": 0,\n    \"fallback\": { \"algorithm\": \"PBKDF2-SHA512\", \"iterations\": 0 },\n    \"pepperEnabled\": false\n  }\n}", null, null, "{\n  \"version\": 1,\n  \"minLength\": 12,\n  \"maxLength\": 128,\n  \"requireUpper\": true,\n  \"requireLower\": true,\n  \"requireDigit\": true,\n  \"requireSymbol\": true,\n  \"allowedSymbols\": \"!@#$%^&*_-+=:?.,;\",\n  \"minDistinctChars\": 5,\n  \"maxRepeatedSequence\": 3,\n  \"blockList\": [\"password\", \"123456\", \"qwerty\", \"admin\"],\n  \"historyCount\": 10,\n  \"maxPasswordAgeDays\": null,\n  \"lockoutThreshold\": 5,\n  \"lockoutSeconds\": 900,\n  \"hash\": {\n    \"algorithm\": \"Argon2id\",\n    \"memoryKb\": 65536,\n    \"parallelism\": 2,\n    \"iterations\": 3,\n    \"saltLength\": 16,\n    \"hashLength\": 32,\n    \"fallback\": { \"algorithm\": \"PBKDF2-SHA512\", \"iterations\": 210000 },\n    \"pepperEnabled\": true\n  }\n}" }
                });

            migrationBuilder.InsertData(
                table: "PasswordBlacklists",
                columns: new[] { "Id", "ApplicationId", "CreatedAt", "CreatedBy", "IsProtected", "LastStatusBy", "StatusId", "UpdatedAt", "UpdatedBy", "Word" },
                values: new object[] { 1, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, 3, null, null, "password" });

            migrationBuilder.InsertData(
                table: "PasswordBlacklists",
                columns: new[] { "Id", "ApplicationId", "CreatedAt", "CreatedBy", "IsProtected", "LastStatusBy", "RowId", "StatusId", "UpdatedAt", "UpdatedBy", "Word" },
                values: new object[,]
                {
                    { 2, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 1, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000001"), 3, null, null, "123456" },
                    { 3, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 2, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000002"), 3, null, null, "12345678" },
                    { 4, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 3, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000003"), 3, null, null, "qwerty" },
                    { 5, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 4, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000004"), 3, null, null, "abc123" },
                    { 6, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 5, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000005"), 3, null, null, "123456789" },
                    { 7, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 6, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000006"), 3, null, null, "111111" },
                    { 8, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 7, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000007"), 3, null, null, "1234567" },
                    { 9, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 8, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000008"), 3, null, null, "letmein" },
                    { 10, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 9, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000009"), 3, null, null, "welcome" },
                    { 11, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 10, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000010"), 3, null, null, "monkey" },
                    { 12, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 11, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000011"), 3, null, null, "1234567890" },
                    { 13, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 12, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000012"), 3, null, null, "dragon" },
                    { 14, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 13, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000013"), 3, null, null, "master" },
                    { 15, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 14, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000014"), 3, null, null, "sunshine" },
                    { 16, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 15, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000015"), 3, null, null, "princess" },
                    { 17, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 16, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000016"), 3, null, null, "qazwsx" },
                    { 18, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 17, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000017"), 3, null, null, "654321" },
                    { 19, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 18, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000018"), 3, null, null, "michael" },
                    { 20, 1, new DateTimeOffset(new DateTime(2025, 12, 11, 10, 0, 19, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, false, 0, new Guid("00000000-0000-0000-0000-000000000019"), 3, null, null, "football" }
                });

            migrationBuilder.InsertData(
                table: "UserApplications",
                columns: new[] { "Id", "ApplicationId", "CreatedBy", "IsProtected", "LastStatusBy", "StatusId", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[] { 1, 1, 0, true, 0, 3, null, null, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Code",
                table: "Applications",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_StatusId",
                table: "Applications",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionAudits_AttemptedAt",
                table: "ConnectionAudits",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionAudits_CorrelationId",
                table: "ConnectionAudits",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionAudits_Result",
                table: "ConnectionAudits",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionAudits_StatusId",
                table: "ConnectionAudits",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionServerWhitelists_Cidr_IsActive",
                table: "ConnectionServerWhitelists",
                columns: new[] { "Cidr", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionServerWhitelists_EnvScope",
                table: "ConnectionServerWhitelists",
                column: "EnvScope");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionServerWhitelists_ServerName_IsActive",
                table: "ConnectionServerWhitelists",
                columns: new[] { "ServerName", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionServerWhitelists_StatusId",
                table: "ConnectionServerWhitelists",
                column: "StatusId");

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
                name: "IX_ParameterDataTypes_Code",
                table: "ParameterDataTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParameterDataTypes_Name",
                table: "ParameterDataTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParameterDataTypes_StatusId",
                table: "ParameterDataTypes",
                column: "StatusId");

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
                name: "IX_Parameters_ParameterDataTypeId",
                table: "Parameters",
                column: "ParameterDataTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Parameters_StatusId",
                table: "Parameters",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordBlacklists_ApplicationId_Word",
                table: "PasswordBlacklists",
                columns: new[] { "ApplicationId", "Word" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordBlacklists_StatusId",
                table: "PasswordBlacklists",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordPolicyAudits_ApplicationId",
                table: "PasswordPolicyAudits",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordPolicyAudits_StatusId",
                table: "PasswordPolicyAudits",
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

            migrationBuilder.CreateIndex(
                name: "IX_UserApplications_ApplicationId",
                table: "UserApplications",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserApplications_StatusId",
                table: "UserApplications",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_UserApplications_UserId_ApplicationId",
                table: "UserApplications",
                columns: new[] { "UserId", "ApplicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPasswordHistories_StatusId",
                table: "UserPasswordHistories",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPasswordHistories_UserId",
                table: "UserPasswordHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPasswordHistories_UserId_CreatedAtUtc",
                table: "UserPasswordHistories",
                columns: new[] { "UserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedUserName",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_StatusId",
                table: "Users",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConnectionAudits");

            migrationBuilder.DropTable(
                name: "ConnectionServerWhitelists");

            migrationBuilder.DropTable(
                name: "FilterItems");

            migrationBuilder.DropTable(
                name: "LanguagePacks");

            migrationBuilder.DropTable(
                name: "Parameters");

            migrationBuilder.DropTable(
                name: "PasswordBlacklists");

            migrationBuilder.DropTable(
                name: "PasswordPolicyAudits");

            migrationBuilder.DropTable(
                name: "UserApplications");

            migrationBuilder.DropTable(
                name: "UserPasswordHistories");

            migrationBuilder.DropTable(
                name: "ParameterDataTypes");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Status");
        }
    }
}
