using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetTrackingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCouplesFeatures2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MyShareRatio",
                table: "Transactions",
                type: "decimal(5, 4)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MyShareRatio",
                table: "Transactions");
        }
    }
}
