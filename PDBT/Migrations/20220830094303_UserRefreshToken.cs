using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PDBT.Migrations
{
    public partial class UserRefreshToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "VerifiedAt",
                table: "Users",
                newName: "RefreshTokenExpires");

            migrationBuilder.RenameColumn(
                name: "VerificationToken",
                table: "Users",
                newName: "RefreshToken");

            migrationBuilder.RenameColumn(
                name: "ResetTokenExpires",
                table: "Users",
                newName: "RefreshTokenCreated");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshTokenExpires",
                table: "Users",
                newName: "VerifiedAt");

            migrationBuilder.RenameColumn(
                name: "RefreshTokenCreated",
                table: "Users",
                newName: "ResetTokenExpires");

            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "Users",
                newName: "VerificationToken");

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
