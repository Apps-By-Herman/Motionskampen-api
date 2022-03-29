using Microsoft.EntityFrameworkCore.Migrations;

namespace Moveness.Migrations
{
    public partial class AddedStyff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Challenges",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActivitityChallange",
                columns: table => new
                {
                    ActivityId = table.Column<int>(nullable: false),
                    ChallengeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivitityChallange", x => new { x.ActivityId, x.ChallengeId });
                    table.ForeignKey(
                        name: "FK_ActivitityChallange_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivitityChallange_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_OwnerId",
                table: "Challenges",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitityChallange_ChallengeId",
                table: "ActivitityChallange",
                column: "ChallengeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_AspNetUsers_OwnerId",
                table: "Challenges",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_AspNetUsers_OwnerId",
                table: "Challenges");

            migrationBuilder.DropTable(
                name: "ActivitityChallange");

            migrationBuilder.DropIndex(
                name: "IX_Challenges_OwnerId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Challenges");
        }
    }
}
