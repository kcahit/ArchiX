using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiX.Library.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeDeleteToRestrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // UserApplications → Users FK (Cascade → Restrict)
            migrationBuilder.Sql(@"
                ALTER TABLE [UserApplications] 
                DROP CONSTRAINT [FK_UserApplications_Users_UserId];
                
                ALTER TABLE [UserApplications] 
                ADD CONSTRAINT [FK_UserApplications_Users_UserId] 
                FOREIGN KEY ([UserId]) 
                REFERENCES [Users] ([Id]) 
                ON DELETE NO ACTION;
            ");

            // UserApplications → Applications FK (Cascade → Restrict)
            migrationBuilder.Sql(@"
                ALTER TABLE [UserApplications] 
                DROP CONSTRAINT [FK_UserApplications_Applications_ApplicationId];
                
                ALTER TABLE [UserApplications] 
                ADD CONSTRAINT [FK_UserApplications_Applications_ApplicationId] 
                FOREIGN KEY ([ApplicationId]) 
                REFERENCES [Applications] ([Id]) 
                ON DELETE NO ACTION;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: Restrict → Cascade
            migrationBuilder.Sql(@"
                ALTER TABLE [UserApplications] 
                DROP CONSTRAINT [FK_UserApplications_Users_UserId];
                
                ALTER TABLE [UserApplications] 
                ADD CONSTRAINT [FK_UserApplications_Users_UserId] 
                FOREIGN KEY ([UserId]) 
                REFERENCES [Users] ([Id]) 
                ON DELETE CASCADE;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE [UserApplications] 
                DROP CONSTRAINT [FK_UserApplications_Applications_ApplicationId];
                
                ALTER TABLE [UserApplications] 
                ADD CONSTRAINT [FK_UserApplications_Applications_ApplicationId] 
                FOREIGN KEY ([ApplicationId]) 
                REFERENCES [Applications] ([Id]) 
                ON DELETE CASCADE;
            ");
        }
    }
}
