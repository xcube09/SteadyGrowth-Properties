using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteadyGrowth.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFreelancerApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FreelancerApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CurrentResidentialAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: true),
                    PreviousFirmsOrAgencies = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LanguagesSpoken = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SpecResidentialSales = table.Column<bool>(type: "bit", nullable: false),
                    SpecCommercialProperties = table.Column<bool>(type: "bit", nullable: false),
                    SpecOffPlanProjectMarketing = table.Column<bool>(type: "bit", nullable: false),
                    SpecLeadGeneration = table.Column<bool>(type: "bit", nullable: false),
                    SpecSocialMediaAdvertising = table.Column<bool>(type: "bit", nullable: false),
                    SpecOthers = table.Column<bool>(type: "bit", nullable: false),
                    SpecOthersDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SkillClientProspecting = table.Column<bool>(type: "bit", nullable: false),
                    SkillPropertyViewing = table.Column<bool>(type: "bit", nullable: false),
                    SkillMarketKnowledge = table.Column<bool>(type: "bit", nullable: false),
                    Availability = table.Column<int>(type: "int", nullable: false),
                    WillingToWorkOnCommission = table.Column<bool>(type: "bit", nullable: false),
                    SocialMediaLinks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RecentSalesOrCampaignResults = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AcceptedTermsAndDeclaration = table.Column<bool>(type: "bit", nullable: false),
                    ApplicantSignature = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SignatureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreelancerApplications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FreelancerApplications_CreatedAt",
                table: "FreelancerApplications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FreelancerApplications_Email",
                table: "FreelancerApplications",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_FreelancerApplications_Status",
                table: "FreelancerApplications",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FreelancerApplications");
        }
    }
}
