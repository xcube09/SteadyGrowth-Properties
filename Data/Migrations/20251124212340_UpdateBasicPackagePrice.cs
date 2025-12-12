using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteadyGrowth.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBasicPackagePrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE AcademyPackages SET Price = 50.00 WHERE Name = 'Basic Package'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE AcademyPackages SET Price = 0.00 WHERE Name = 'Basic Package'");
        }
    }
}
