using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSysAPI.Migrations
{
    /// <inheritdoc />
    public partial class CreateLoanApplicationTypesTable_Clean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // YENİ TABLO OLUŞTURMA SATIRLARINI KALDIR veya YORUMA AL
            /*
            migrationBuilder.CreateTable(
                name: "LoanApplicationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanApplicationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoanStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanStatuses", x => x.Id);
                });
            */

            // SADECE İNDEKS ve FOREIGN KEY EKLEMELERİ KALSIN:
            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_LoanApplicationTypeId",
                table: "LoanApplications",
                column: "LoanApplicationTypeId");

            /*migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_LoanStatusId",
                table: "LoanApplications",
                column: "LoanStatusId");*/

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplications_LoanApplicationTypes_LoanApplicationTypeId",
                table: "LoanApplications",
                column: "LoanApplicationTypeId",
                principalTable: "LoanApplicationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

           /* migrationBuilder.AddForeignKey(
                name: "FK_LoanApplications_LoanStatuses_LoanStatusId",
                table: "LoanApplications",
                column: "LoanStatusId",
                principalTable: "LoanStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);*/
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplications_LoanApplicationTypes_LoanApplicationTypeId",
                table: "LoanApplications");

            /*migrationBuilder.DropForeignKey(
                name: "FK_LoanApplications_LoanStatuses_LoanStatusId",
                table: "LoanApplications");*/

            //migrationBuilder.DropTable(
                //name: "LoanApplicationTypes");

            //migrationBuilder.DropTable(
                //name: "LoanStatuses");

            migrationBuilder.DropIndex(
                name: "IX_LoanApplications_LoanApplicationTypeId",
                table: "LoanApplications");

            /*migrationBuilder.DropIndex(
                name: "IX_LoanApplications_LoanStatusId",
                table: "LoanApplications");*/

            /*migrationBuilder.DropColumn(
                name: "LoanApplicationTypeId",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "LoanStatusId",
                table: "LoanApplications");
*/
            migrationBuilder.AddColumn<string>(
                name: "LoanType",
                table: "LoanApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
