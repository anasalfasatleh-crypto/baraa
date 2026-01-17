using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "materials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileExtension = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "material_accesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    Completed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_material_accesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_material_accesses_materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_material_accesses_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_material_accesses_AccessedAt",
                table: "material_accesses",
                column: "AccessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_material_accesses_MaterialId",
                table: "material_accesses",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_material_accesses_UserId_MaterialId",
                table: "material_accesses",
                columns: new[] { "UserId", "MaterialId" });

            migrationBuilder.CreateIndex(
                name: "IX_materials_IsActive",
                table: "materials",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_materials_OrderIndex",
                table: "materials",
                column: "OrderIndex");

            migrationBuilder.CreateIndex(
                name: "IX_materials_Type",
                table: "materials",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "material_accesses");

            migrationBuilder.DropTable(
                name: "materials");
        }
    }
}
