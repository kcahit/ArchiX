using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class TwoFactorUpdateFullChannels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Database'deki mevcut veriyi güncelle (Snapshot zaten doðru)
            migrationBuilder.Sql(@"
                UPDATE [Parameters]
                SET [Value] = N'{
  ""defaultChannel"": ""Email"",
  ""channels"": {
    ""Sms"": { ""codeLength"": 6, ""expirySeconds"": 300 },
    ""Email"": { ""codeLength"": 6, ""expirySeconds"": 300 },
    ""Authenticator"": { ""digits"": 6, ""periodSeconds"": 30, ""hashAlgorithm"": ""SHA1"" }
  }
}'
                WHERE [Id] = 1 
                  AND [Group] = 'TwoFactor' 
                  AND [Key] = 'Options';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri al
            migrationBuilder.Sql(@"
                UPDATE [Parameters]
                SET [Value] = N'{""defaultChannel"":""Sms""}'
                WHERE [Id] = 1 
                  AND [Group] = 'TwoFactor' 
                  AND [Key] = 'Options';
            ");
        }
    }
}

