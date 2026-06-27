using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteadyGrowth.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingRegistrationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelectedPackageId = table.Column<int>(type: "int", nullable: false),
                    ReferralCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingRegistrations_AcademyPackages_SelectedPackageId",
                        column: x => x.SelectedPackageId,
                        principalTable: "AcademyPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PendingRegistrations_AspNetUsers_CreatedUserId",
                        column: x => x.CreatedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PendingRegistrations_AspNetUsers_ProcessedByUserId",
                        column: x => x.ProcessedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingRegistrations_CreatedAt",
                table: "PendingRegistrations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PendingRegistrations_CreatedUserId",
                table: "PendingRegistrations",
                column: "CreatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingRegistrations_Email",
                table: "PendingRegistrations",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_PendingRegistrations_IsDeleted",
                table: "PendingRegistrations",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PendingRegistrations_ProcessedByUserId",
                table: "PendingRegistrations",
                column: "ProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingRegistrations_SelectedPackageId",
                table: "PendingRegistrations",
                column: "SelectedPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingRegistrations_Status",
                table: "PendingRegistrations",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingRegistrations");
        }
    }
}
