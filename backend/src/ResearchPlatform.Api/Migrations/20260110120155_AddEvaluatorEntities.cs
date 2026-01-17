using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEvaluatorEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "combined_scores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionnaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    AverageScore = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    EvaluatorCount = table.Column<int>(type: "integer", nullable: false),
                    IsFinalized = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_combined_scores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_combined_scores_questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_combined_scores_questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_combined_scores_users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "evaluator_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evaluator_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_evaluator_assignments_users_EvaluatorId",
                        column: x => x.EvaluatorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_evaluator_assignments_users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "evaluator_scores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionnaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    IsFinalized = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evaluator_scores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_evaluator_scores_questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_evaluator_scores_questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_evaluator_scores_users_EvaluatorId",
                        column: x => x.EvaluatorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_evaluator_scores_users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_combined_scores_IsFinalized",
                table: "combined_scores",
                column: "IsFinalized");

            migrationBuilder.CreateIndex(
                name: "IX_combined_scores_QuestionId",
                table: "combined_scores",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_combined_scores_QuestionnaireId",
                table: "combined_scores",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_combined_scores_StudentId_QuestionnaireId_QuestionId",
                table: "combined_scores",
                columns: new[] { "StudentId", "QuestionnaireId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_evaluator_assignments_EvaluatorId_StudentId",
                table: "evaluator_assignments",
                columns: new[] { "EvaluatorId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_evaluator_assignments_IsActive",
                table: "evaluator_assignments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_evaluator_assignments_StudentId",
                table: "evaluator_assignments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_evaluator_scores_EvaluatorId",
                table: "evaluator_scores",
                column: "EvaluatorId");

            migrationBuilder.CreateIndex(
                name: "IX_evaluator_scores_IsFinalized",
                table: "evaluator_scores",
                column: "IsFinalized");

            migrationBuilder.CreateIndex(
                name: "IX_evaluator_scores_QuestionId",
                table: "evaluator_scores",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_evaluator_scores_QuestionnaireId",
                table: "evaluator_scores",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_evaluator_scores_StudentId_QuestionnaireId_QuestionId_Evalu~",
                table: "evaluator_scores",
                columns: new[] { "StudentId", "QuestionnaireId", "QuestionId", "EvaluatorId" },
                unique: true);

            // Add trigger to prevent updates to finalized evaluator scores
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION prevent_finalized_evaluator_score_update()
                RETURNS TRIGGER AS $$
                BEGIN
                    IF OLD.""IsFinalized"" = TRUE AND NEW.""IsFinalized"" = TRUE THEN
                        RAISE EXCEPTION 'Cannot update finalized evaluator score';
                    END IF;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER prevent_evaluator_score_update
                BEFORE UPDATE ON evaluator_scores
                FOR EACH ROW
                EXECUTE FUNCTION prevent_finalized_evaluator_score_update();
            ");

            // Add trigger to prevent updates to finalized combined scores
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION prevent_finalized_combined_score_update()
                RETURNS TRIGGER AS $$
                BEGIN
                    IF OLD.""IsFinalized"" = TRUE AND NEW.""IsFinalized"" = TRUE THEN
                        RAISE EXCEPTION 'Cannot update finalized combined score';
                    END IF;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER prevent_combined_score_update
                BEFORE UPDATE ON combined_scores
                FOR EACH ROW
                EXECUTE FUNCTION prevent_finalized_combined_score_update();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop triggers
            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS prevent_evaluator_score_update ON evaluator_scores;
                DROP FUNCTION IF EXISTS prevent_finalized_evaluator_score_update();
                DROP TRIGGER IF EXISTS prevent_combined_score_update ON combined_scores;
                DROP FUNCTION IF EXISTS prevent_finalized_combined_score_update();
            ");

            migrationBuilder.DropTable(
                name: "combined_scores");

            migrationBuilder.DropTable(
                name: "evaluator_assignments");

            migrationBuilder.DropTable(
                name: "evaluator_scores");
        }
    }
}
