using Microsoft.EntityFrameworkCore.Migrations;

namespace Moveness.Migrations
{
    public partial class AddedLotsOfStuff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChallengedId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengedName",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengedScore",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengingId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengingName",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengingScore",
                table: "Challenges");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Teams",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChallengedTeamId",
                table: "Challenges",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ChallengedUserId",
                table: "Challenges",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChallengingTeamId",
                table: "Challenges",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ChallengingUserId",
                table: "Challenges",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTeamChallenge",
                table: "Challenges",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TeamChallenge",
                columns: table => new
                {
                    TeamId = table.Column<int>(nullable: false),
                    ChallengeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamChallenge", x => new { x.TeamId, x.ChallengeId });
                    table.ForeignKey(
                        name: "FK_TeamChallenge_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamChallenge_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_OwnerId",
                table: "Teams",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamChallenge_ChallengeId",
                table: "TeamChallenge",
                column: "ChallengeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_AspNetUsers_OwnerId",
                table: "Teams",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_AspNetUsers_OwnerId",
                table: "Teams");

            migrationBuilder.DropTable(
                name: "TeamChallenge");

            migrationBuilder.DropIndex(
                name: "IX_Teams_OwnerId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "ChallengedTeamId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengedUserId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengingTeamId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengingUserId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "IsTeamChallenge",
                table: "Challenges");

            migrationBuilder.AddColumn<string>(
                name: "ChallengedId",
                table: "Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChallengedName",
                table: "Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChallengedScore",
                table: "Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ChallengingId",
                table: "Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChallengingName",
                table: "Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChallengingScore",
                table: "Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
