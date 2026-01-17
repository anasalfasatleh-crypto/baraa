using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionnaireEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "questionnaires",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionnaires", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionnaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Options = table.Column<string>(type: "jsonb", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Step = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    MinValue = table.Column<int>(type: "integer", nullable: true),
                    MaxValue = table.Column<int>(type: "integer", nullable: true),
                    MinLabel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MaxLabel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_questions_questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "step_timings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionnaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    Step = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeSpentSeconds = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_step_timings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_step_timings_questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_step_timings_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionnaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    IsSubmitted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_answers_questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_answers_questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_answers_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionnaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AutoScore = table.Column<decimal>(type: "numeric", nullable: true),
                    ManualScore = table.Column<decimal>(type: "numeric", nullable: true),
                    EvaluatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    EvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EvaluatorNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_scores_questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_scores_questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_scores_users_EvaluatedBy",
                        column: x => x.EvaluatedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_scores_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_answers_IsSubmitted",
                table: "answers",
                column: "IsSubmitted");

            migrationBuilder.CreateIndex(
                name: "IX_answers_QuestionId",
                table: "answers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_answers_QuestionnaireId",
                table: "answers",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_answers_UserId_QuestionnaireId",
                table: "answers",
                columns: new[] { "UserId", "QuestionnaireId" });

            migrationBuilder.CreateIndex(
                name: "IX_answers_UserId_QuestionnaireId_QuestionId",
                table: "answers",
                columns: new[] { "UserId", "QuestionnaireId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_questionnaires_IsActive",
                table: "questionnaires",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_questionnaires_Type",
                table: "questionnaires",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_questions_QuestionnaireId_OrderIndex",
                table: "questions",
                columns: new[] { "QuestionnaireId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_questions_QuestionnaireId_Step",
                table: "questions",
                columns: new[] { "QuestionnaireId", "Step" });

            migrationBuilder.CreateIndex(
                name: "IX_scores_EvaluatedBy",
                table: "scores",
                column: "EvaluatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_scores_QuestionId",
                table: "scores",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_scores_QuestionnaireId",
                table: "scores",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_scores_UserId_QuestionnaireId",
                table: "scores",
                columns: new[] { "UserId", "QuestionnaireId" });

            migrationBuilder.CreateIndex(
                name: "IX_scores_UserId_QuestionnaireId_QuestionId",
                table: "scores",
                columns: new[] { "UserId", "QuestionnaireId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_step_timings_QuestionnaireId",
                table: "step_timings",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_step_timings_UserId_QuestionnaireId_Step",
                table: "step_timings",
                columns: new[] { "UserId", "QuestionnaireId", "Step" });

            // Create trigger function to prevent updates to submitted answers
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION prevent_submitted_answer_update()
                RETURNS TRIGGER AS $$
                BEGIN
                    IF OLD.""IsSubmitted"" = TRUE THEN
                        RAISE EXCEPTION 'Cannot modify submitted answers';
                    END IF;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Create trigger on answers table
            migrationBuilder.Sql(@"
                CREATE TRIGGER prevent_answer_update
                BEFORE UPDATE ON answers
                FOR EACH ROW
                EXECUTE FUNCTION prevent_submitted_answer_update();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop trigger and function
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS prevent_answer_update ON answers;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS prevent_submitted_answer_update();");

            migrationBuilder.DropTable(
                name: "answers");

            migrationBuilder.DropTable(
                name: "scores");

            migrationBuilder.DropTable(
                name: "step_timings");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "questionnaires");
        }
    }
}
