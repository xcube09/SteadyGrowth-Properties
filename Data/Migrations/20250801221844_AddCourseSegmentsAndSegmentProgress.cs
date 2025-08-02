using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteadyGrowth.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseSegmentsAndSegmentProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseSegments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseSegments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseSegmentId = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastAccessedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SegmentProgresses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SegmentProgresses_CourseSegments_CourseSegmentId",
                        column: x => x.CourseSegmentId,
                        principalTable: "CourseSegments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseSegments_CourseId",
                table: "CourseSegments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSegments_IsActive",
                table: "CourseSegments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSegments_Order",
                table: "CourseSegments",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentProgresses_CourseSegmentId",
                table: "SegmentProgresses",
                column: "CourseSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentProgresses_IsCompleted",
                table: "SegmentProgresses",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentProgresses_UserId",
                table: "SegmentProgresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentProgresses_UserId_CourseSegmentId",
                table: "SegmentProgresses",
                columns: new[] { "UserId", "CourseSegmentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SegmentProgresses");

            migrationBuilder.DropTable(
                name: "CourseSegments");
        }
    }
}
