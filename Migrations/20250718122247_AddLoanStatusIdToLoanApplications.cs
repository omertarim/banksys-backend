using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSysAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanStatusIdToLoanApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoanStatusId",
                table: "LoanApplications",
                type: "int",
                nullable: false,
                defaultValue: -1 // Use -1 to match your "Pending" status
            );

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_LoanStatusId",
                table: "LoanApplications",
                column: "LoanStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplications_LoanStatuses_LoanStatusId",
                table: "LoanApplications",
                column: "LoanStatusId",
                principalTable: "LoanStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplications_LoanStatuses_LoanStatusId",
                table: "LoanApplications");

            migrationBuilder.DropIndex(
                name: "IX_LoanApplications_LoanStatusId",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "LoanStatusId",
                table: "LoanApplications");
        }
    }
}
