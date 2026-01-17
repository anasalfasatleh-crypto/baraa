using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPostTestBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "post_test_batches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OpenDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CloseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_test_batches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_post_test_batches_IsActive",
                table: "post_test_batches",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_post_test_batches_OpenDate_CloseDate",
                table: "post_test_batches",
                columns: new[] { "OpenDate", "CloseDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "post_test_batches");
        }
    }
}
