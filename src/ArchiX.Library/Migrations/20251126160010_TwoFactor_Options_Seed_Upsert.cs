using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class TwoFactor_Options_Seed_Upsert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM [Parameters] WHERE [ApplicationId] = 1 AND [Group] = 'TwoFactor' AND [Key] = 'Options')
                BEGIN
                    UPDATE [Parameters]
                       SET [Value] = N'{"defaultChannel":"Sms"}',
                           [ParameterDataTypeId] = 15
                     WHERE [ApplicationId] = 1 AND [Group] = 'TwoFactor' AND [Key] = 'Options';
                END
                ELSE
                BEGIN
                    INSERT INTO [Parameters] ([ApplicationId], [Group], [Key], [ParameterDataTypeId], [Value], [Description], [StatusId], [CreatedBy])
                    VALUES (1, 'TwoFactor', 'Options', 15, N'{"defaultChannel":"Sms"}', N'Ýki faktör seçenekleri (varsayýlan SMS)', 3, 0);
                END
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM [Parameters] WHERE [ApplicationId] = 1 AND [Group] = 'TwoFactor' AND [Key] = 'Options')
                BEGIN
                    UPDATE [Parameters]
                       SET [Value] = N'{"defaultChannel":"Email"}'
                     WHERE [ApplicationId] = 1 AND [Group] = 'TwoFactor' AND [Key] = 'Options';
                END
                """
            );
        }
    }
}
