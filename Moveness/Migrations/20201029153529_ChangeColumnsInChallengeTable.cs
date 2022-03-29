using Microsoft.EntityFrameworkCore.Migrations;

namespace Moveness.Migrations
{
    public partial class ChangeColumnsInChallengeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChallengedTeamId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengedTeamName",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengedTeamScore",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengingTeamId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengingTeamName",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengingTeamScore",
                table: "Challenges");

            migrationBuilder.AddColumn<string>(
                name: "ChallengedId",
                table: "Challenges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChallengedName",
                table: "Challenges",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChallengedScore",
                table: "Challenges",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ChallengingId",
                table: "Challenges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChallengingName",
                table: "Challenges",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChallengingScore",
                table: "Challenges",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "ChallengedTeamId",
                table: "Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ChallengedTeamName",
                table: "Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChallengedTeamScore",
                table: "Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ChallengingTeamId",
                table: "Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ChallengingTeamName",
                table: "Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChallengingTeamScore",
                table: "Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
