using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PsiArtigos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PersonalizedRecommendations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "EngagementScore",
                table: "Recommendations",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAtUtc",
                table: "Recommendations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "FreshnessScore",
                table: "Recommendations",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "QualityScore",
                table: "Recommendations",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TopicScore",
                table: "Recommendations",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ActiveReadingSeconds",
                table: "ReadingSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OpenCount",
                table: "ReadingSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EngagementScore",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "ExpiresAtUtc",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "FreshnessScore",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "QualityScore",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "TopicScore",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "ActiveReadingSeconds",
                table: "ReadingSessions");

            migrationBuilder.DropColumn(
                name: "OpenCount",
                table: "ReadingSessions");
        }
    }
}
