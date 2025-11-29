using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class PasswordPolicy_Seed_Default : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM [Parameters] WHERE [ApplicationId] = 1 AND [Group] = 'Security' AND [Key] = 'PasswordPolicy')
                BEGIN
                    UPDATE [Parameters]
                       SET [Value] = N'{
                         "version": 1,
                         "minLength": 12,
                         "maxLength": 128,
                         "requireUpper": true,
                         "requireLower": true,
                         "requireDigit": true,
                         "requireSymbol": true,
                         "allowedSymbols": "!@#$%^&*_-+=:?.,;",
                         "minDistinctChars": 5,
                         "maxRepeatedSequence": 3,
                         "blockList": ["password","123456","qwerty","admin"],
                         "historyCount": 10,
                         "lockoutThreshold": 5,
                         "lockoutSeconds": 900,
                         "hash": {
                           "algorithm": "Argon2id",
                           "memoryKb": 65536,
                           "parallelism": 2,
                           "iterations": 3,
                           "saltLength": 16,
                           "hashLength": 32,
                           "fallback": { "algorithm": "PBKDF2-SHA512", "iterations": 210000 },
                           "pepperEnabled": false
                         }
                       }',
                           [ParameterDataTypeId] = 15
                     WHERE [ApplicationId] = 1 AND [Group] = 'Security' AND [Key] = 'PasswordPolicy';
                END
                ELSE
                BEGIN
                    INSERT INTO [Parameters] (
                        [ApplicationId], [Group], [Key], [ParameterDataTypeId], [Value], [Description], [StatusId], [CreatedBy]
                    )
                    VALUES (
                        1, 'Security', 'PasswordPolicy', 15, N'{
                          "version": 1,
                          "minLength": 12,
                          "maxLength": 128,
                          "requireUpper": true,
                          "requireLower": true,
                          "requireDigit": true,
                          "requireSymbol": true,
                          "allowedSymbols": "!@#$%^&*_-+=:?.,;",
                          "minDistinctChars": 5,
                          "maxRepeatedSequence": 3,
                          "blockList": ["password","123456","qwerty","admin"],
                          "historyCount": 10,
                          "lockoutThreshold": 5,
                          "lockoutSeconds": 900,
                          "hash": {
                            "algorithm": "Argon2id",
                            "memoryKb": 65536,
                            "parallelism": 2,
                            "iterations": 3,
                            "saltLength": 16,
                            "hashLength": 32,
                            "fallback": { "algorithm": "PBKDF2-SHA512", "iterations": 210000 },
                            "pepperEnabled": false
                          }
                        }',
                        N'Parola politikasý (varsayýlan)', 3, 0
                    );
                END
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM [Parameters]
                 WHERE [ApplicationId] = 1 AND [Group] = 'Security' AND [Key] = 'PasswordPolicy';
                """
            );
        }
    }
}
