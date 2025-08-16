using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteadyGrowth.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyToUSDOnlyFinances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove currency fields from entities that should only handle USD
            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ExchangeRateAtCreation",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "ExchangeRateAtCreation",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "AcademyPackages");

            // Note: Keep CurrencyCode on Properties table for user flexibility in property listings
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "Wallets",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "WalletTransactions",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "WalletTransactions",
                type: "decimal(18,6)",
                nullable: true);

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
                table: "WithdrawalRequests",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRateAtCreation",
                table: "WithdrawalRequests",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "AcademyPackages",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);
        }
    }
}