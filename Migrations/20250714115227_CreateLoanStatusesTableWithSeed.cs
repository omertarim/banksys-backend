using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BankSysAPI.Migrations
{
    /// <inheritdoc />
    public partial class CreateLoanStatusesTableWithSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "LoanStatuses",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { -3, "Loan has been rejected", true, "Rejected" },
                    { -2, "Loan has been approved", true, "Approved" },
                    { -1, "Waiting for approval", true, "Pending" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LoanStatuses",
                keyColumn: "Id",
                keyValue: -3);

            migrationBuilder.DeleteData(
                table: "LoanStatuses",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "LoanStatuses",
                keyColumn: "Id",
                keyValue: -1);
        }
    }
}
