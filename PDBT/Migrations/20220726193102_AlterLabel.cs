using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PDBT.Migrations
{
    public partial class AlterLabel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Labels_Issues_IssueId",
                table: "Labels");

            migrationBuilder.DropIndex(
                name: "IX_Labels_IssueId",
                table: "Labels");

            migrationBuilder.DropColumn(
                name: "IssueId",
                table: "Labels");

            migrationBuilder.CreateTable(
                name: "LabelDetail",
                columns: table => new
                {
                    IssueId = table.Column<int>(type: "int", nullable: false),
                    LabelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelDetail", x => new { x.IssueId, x.LabelId });
                    table.ForeignKey(
                        name: "FK_LabelDetail_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabelDetail_Labels_LabelId",
                        column: x => x.LabelId,
                        principalTable: "Labels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_LabelDetail_LabelId",
                table: "LabelDetail",
                column: "LabelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabelDetail");

            migrationBuilder.AddColumn<int>(
                name: "IssueId",
                table: "Labels",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Labels_IssueId",
                table: "Labels",
                column: "IssueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Labels_Issues_IssueId",
                table: "Labels",
                column: "IssueId",
                principalTable: "Issues",
                principalColumn: "Id");
        }
    }
}
