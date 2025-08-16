using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteadyGrowth.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyToRemainingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "Referrals",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRateAtCreation",
                table: "Referrals",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "AcademyPackages",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "WithdrawalRequests",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRateAtCreation",
                table: "WithdrawalRequests",
                type: "decimal(18,6)",
                nullable: true);

            // Set default currency for existing records
            migrationBuilder.Sql(@"
                UPDATE Referrals SET CurrencyCode = 'NGN' WHERE CurrencyCode IS NULL;
                UPDATE AcademyPackages SET CurrencyCode = 'NGN' WHERE CurrencyCode IS NULL;
                UPDATE WithdrawalRequests SET CurrencyCode = 'NGN' WHERE CurrencyCode IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ExchangeRateAtCreation",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "AcademyPackages");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "ExchangeRateAtCreation",
                table: "WithdrawalRequests");
        }
    }
}