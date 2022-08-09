using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PDBT.Migrations
{
    public partial class UserAndProject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Project_RootProjectID",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_IssueUser_User_AssigneesId",
                table: "IssueUser");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectUser_Project_ProjectsId",
                table: "ProjectUser");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectUser_User_UsersId",
                table: "ProjectUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Project",
                table: "Project");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "Project",
                newName: "Projects");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Projects",
                table: "Projects",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Projects_RootProjectID",
                table: "Issues",
                column: "RootProjectID",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IssueUser_Users_AssigneesId",
                table: "IssueUser",
                column: "AssigneesId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectUser_Projects_ProjectsId",
                table: "ProjectUser",
                column: "ProjectsId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectUser_Users_UsersId",
                table: "ProjectUser",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Projects_RootProjectID",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_IssueUser_Users_AssigneesId",
                table: "IssueUser");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectUser_Projects_ProjectsId",
                table: "ProjectUser");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectUser_Users_UsersId",
                table: "ProjectUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Projects",
                table: "Projects");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "Projects",
                newName: "Project");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Project",
                table: "Project",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Project_RootProjectID",
                table: "Issues",
                column: "RootProjectID",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IssueUser_User_AssigneesId",
                table: "IssueUser",
                column: "AssigneesId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectUser_Project_ProjectsId",
                table: "ProjectUser",
                column: "ProjectsId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectUser_User_UsersId",
                table: "ProjectUser",
                column: "UsersId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
