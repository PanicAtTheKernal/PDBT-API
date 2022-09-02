using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PDBT.Migrations
{
    public partial class ProjectSpecificLabels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RootProjectId",
                table: "Labels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Labels_RootProjectId",
                table: "Labels",
                column: "RootProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Labels_Projects_RootProjectId",
                table: "Labels",
                column: "RootProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Labels_Projects_RootProjectId",
                table: "Labels");

            migrationBuilder.DropIndex(
                name: "IX_Labels_RootProjectId",
                table: "Labels");

            migrationBuilder.DropColumn(
                name: "RootProjectId",
                table: "Labels");
        }
    }
}
