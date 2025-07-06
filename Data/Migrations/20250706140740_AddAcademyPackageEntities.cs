using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteadyGrowth.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademyPackageEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcademyPackageId",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademyPackageId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AcademyPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademyPackages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_AcademyPackageId",
                table: "Courses",
                column: "AcademyPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AcademyPackageId",
                table: "AspNetUsers",
                column: "AcademyPackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AcademyPackages_AcademyPackageId",
                table: "AspNetUsers",
                column: "AcademyPackageId",
                principalTable: "AcademyPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_AcademyPackages_AcademyPackageId",
                table: "Courses",
                column: "AcademyPackageId",
                principalTable: "AcademyPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AcademyPackages_AcademyPackageId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_AcademyPackages_AcademyPackageId",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "AcademyPackages");

            migrationBuilder.DropIndex(
                name: "IX_Courses_AcademyPackageId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AcademyPackageId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AcademyPackageId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "AcademyPackageId",
                table: "AspNetUsers");
        }
    }
}
