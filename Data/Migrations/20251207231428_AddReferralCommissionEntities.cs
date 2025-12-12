using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteadyGrowth.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralCommissionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReferralCommissionLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    AcademyPackageId = table.Column<int>(type: "int", nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralCommissionLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferralCommissionLevels_AcademyPackages_AcademyPackageId",
                        column: x => x.AcademyPackageId,
                        principalTable: "AcademyPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReferralCommissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BeneficiaryUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SourceUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AcademyPackageId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WalletTransactionId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralCommissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferralCommissions_AcademyPackages_AcademyPackageId",
                        column: x => x.AcademyPackageId,
                        principalTable: "AcademyPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReferralCommissions_AspNetUsers_BeneficiaryUserId",
                        column: x => x.BeneficiaryUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReferralCommissions_AspNetUsers_SourceUserId",
                        column: x => x.SourceUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReferralCommissions_WalletTransactions_WalletTransactionId",
                        column: x => x.WalletTransactionId,
                        principalTable: "WalletTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReferralCommissionLevels_AcademyPackageId",
                table: "ReferralCommissionLevels",
                column: "AcademyPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralCommissionLevels_IsActive",
                table: "ReferralCommissionLevels",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralCommissionLevels_Level_AcademyPackageId",
                table: "ReferralCommissionLevels",
                columns: new[] { "Level", "AcademyPackageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferralCommissions_AcademyPackageId",
                table: "ReferralCommissions",
                column: "AcademyPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralCommissions_BeneficiaryUserId",
                table: "ReferralCommissions",
                column: "BeneficiaryUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralCommissions_CreatedAt",
                table: "ReferralCommissions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralCommissions_SourceUserId",
                table: "ReferralCommissions",
                column: "SourceUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralCommissions_WalletTransactionId",
                table: "ReferralCommissions",
                column: "WalletTransactionId");

            // Seed referral commission levels
            // Level 1 (Direct): Basic=$10, Premium=$30
            // Level 2 (Indirect): Basic=$3, Premium=$7
            migrationBuilder.Sql(@"
                INSERT INTO ReferralCommissionLevels (Level, AcademyPackageId, CommissionAmount, IsActive, CreatedAt)
                SELECT 1, Id, 10.00, 1, GETUTCDATE() FROM AcademyPackages WHERE Name = 'Basic Package'
                UNION ALL
                SELECT 1, Id, 30.00, 1, GETUTCDATE() FROM AcademyPackages WHERE Name = 'Premium Package'
                UNION ALL
                SELECT 2, Id, 3.00, 1, GETUTCDATE() FROM AcademyPackages WHERE Name = 'Basic Package'
                UNION ALL
                SELECT 2, Id, 7.00, 1, GETUTCDATE() FROM AcademyPackages WHERE Name = 'Premium Package'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete seed data first (ReferralCommissions has FK to ReferralCommissionLevels)
            migrationBuilder.Sql("DELETE FROM ReferralCommissions");
            migrationBuilder.Sql("DELETE FROM ReferralCommissionLevels");

            migrationBuilder.DropTable(
                name: "ReferralCommissions");

            migrationBuilder.DropTable(
                name: "ReferralCommissionLevels");
        }
    }
}
