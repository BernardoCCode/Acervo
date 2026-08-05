using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PsiArtigos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Abstract = table.Column<string>(type: "text", nullable: true),
                    Venue = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    Doi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PrimarySource = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CitationCount = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    StudyType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PdfUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CitationLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FromArticleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToArticleId = table.Column<Guid>(type: "uuid", nullable: false),
                    LinkType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitationLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArticleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedInsights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArticleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    SourceLanguage = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    TargetLanguage = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedInsights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearningTrails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Prompt = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Topic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningTrails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReadingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArticleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgressPercent = table.Column<double>(type: "double precision", nullable: false),
                    ProgressPageNumber = table.Column<int>(type: "integer", nullable: true),
                    ProgressCharacterOffset = table.Column<int>(type: "integer", nullable: true),
                    LastOpenedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadingSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArticleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Score = table.Column<double>(type: "double precision", nullable: false),
                    Explanation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SourceArticleId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDismissed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchQueries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RawText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FilterYearMin = table.Column<int>(type: "integer", nullable: true),
                    FilterYearMax = table.Column<int>(type: "integer", nullable: true),
                    FilterLanguage = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    FilterStudyType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    FilterMinCitations = table.Column<int>(type: "integer", nullable: true),
                    FilterSourcesJson = table.Column<string>(type: "text", nullable: false),
                    ResultCount = table.Column<int>(type: "integer", nullable: false),
                    ExecutedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastAccessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResultArticleIdsJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchQueries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserReaderSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DarkMode = table.Column<bool>(type: "boolean", nullable: false),
                    FontSize = table.Column<int>(type: "integer", nullable: false),
                    PreferredTranslationLanguage = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReaderSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PreferredLanguage = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticleAuthors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Orcid = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Affiliation = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ArticleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleAuthors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleAuthors_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticleExternalReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    System = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ArticleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleExternalReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleExternalReferences_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticleTopics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ArticleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleTopics_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArticleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionItems_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearningTrailSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Difficulty = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ArticleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Rationale = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LearningTrailId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningTrailSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningTrailSteps_LearningTrails_LearningTrailId",
                        column: x => x.LearningTrailId,
                        principalTable: "LearningTrails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Highlights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartOffset = table.Column<int>(type: "integer", nullable: false),
                    EndOffset = table.Column<int>(type: "integer", nullable: false),
                    PageNumber = table.Column<int>(type: "integer", nullable: true),
                    QuotedText = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReadingSessionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Highlights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Highlights_ReadingSessions_ReadingSessionId",
                        column: x => x.ReadingSessionId,
                        principalTable: "ReadingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserInterests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInterests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInterests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Annotations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HighlightId = table.Column<Guid>(type: "uuid", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Annotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Annotations_Highlights_HighlightId",
                        column: x => x.HighlightId,
                        principalTable: "Highlights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_HighlightId",
                table: "Annotations",
                column: "HighlightId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleAuthors_ArticleId",
                table: "ArticleAuthors",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleExternalReferences_ArticleId",
                table: "ArticleExternalReferences",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleExternalReferences_System_ExternalId",
                table: "ArticleExternalReferences",
                columns: new[] { "System", "ExternalId" });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Doi",
                table: "Articles",
                column: "Doi");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleTopics_ArticleId",
                table: "ArticleTopics",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_CitationLinks_FromArticleId_ToArticleId_LinkType",
                table: "CitationLinks",
                columns: new[] { "FromArticleId", "ToArticleId", "LinkType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionItems_ArticleId",
                table: "CollectionItems",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionItems_CollectionId",
                table: "CollectionItems",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_UserId_Name",
                table: "Collections",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId_ArticleId",
                table: "Favorites",
                columns: new[] { "UserId", "ArticleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedInsights_UserId_ArticleId_Type",
                table: "GeneratedInsights",
                columns: new[] { "UserId", "ArticleId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Highlights_ReadingSessionId",
                table: "Highlights",
                column: "ReadingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningTrailSteps_LearningTrailId",
                table: "LearningTrailSteps",
                column: "LearningTrailId");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingSessions_UserId_ArticleId",
                table: "ReadingSessions",
                columns: new[] { "UserId", "ArticleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_UserId_IsDismissed",
                table: "Recommendations",
                columns: new[] { "UserId", "IsDismissed" });

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_UserId",
                table: "UserInterests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Annotations");

            migrationBuilder.DropTable(
                name: "ArticleAuthors");

            migrationBuilder.DropTable(
                name: "ArticleExternalReferences");

            migrationBuilder.DropTable(
                name: "ArticleTopics");

            migrationBuilder.DropTable(
                name: "CitationLinks");

            migrationBuilder.DropTable(
                name: "CollectionItems");

            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.DropTable(
                name: "GeneratedInsights");

            migrationBuilder.DropTable(
                name: "LearningTrailSteps");

            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "SearchQueries");

            migrationBuilder.DropTable(
                name: "UserInterests");

            migrationBuilder.DropTable(
                name: "UserReaderSettings");

            migrationBuilder.DropTable(
                name: "Highlights");

            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "Collections");

            migrationBuilder.DropTable(
                name: "LearningTrails");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ReadingSessions");
        }
    }
}
