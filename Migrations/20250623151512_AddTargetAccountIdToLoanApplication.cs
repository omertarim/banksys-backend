using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSysAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetAccountIdToLoanApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TargetAccountId kolonunu ekle (nullable olarak)
            migrationBuilder.AddColumn<int>(
                name: "TargetAccountId",
                table: "LoanApplications",
                type: "int",
                nullable: true);

            // Index oluştur
            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_TargetAccountId",
                table: "LoanApplications",
                column: "TargetAccountId");

            // Foreign key oluştur
            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplications_Accounts_TargetAccountId",
                table: "LoanApplications",
                column: "TargetAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Foreign key ve index'i kaldır
            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplications_Accounts_TargetAccountId",
                table: "LoanApplications");

            migrationBuilder.DropIndex(
                name: "IX_LoanApplications_TargetAccountId",
                table: "LoanApplications");

            // Kolonu kaldır
            migrationBuilder.DropColumn(
                name: "TargetAccountId",
                table: "LoanApplications");
        }
    }
}
