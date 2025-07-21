using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSysAPI.Migrations
{
    /// <inheritdoc />
    public partial class SyncPersonTypeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "PersonType",
                table: "Customers");

            migrationBuilder.AddColumn<int>(
                name: "PersonTypeId",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PersonTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PersonTypeId",
                table: "Customers",
                column: "PersonTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_PersonTypes_PersonTypeId",
                table: "Customers",
                column: "PersonTypeId",
                principalTable: "PersonTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_PersonTypes_PersonTypeId",
                table: "Customers");

            migrationBuilder.DropTable(
                name: "PersonTypes");

            migrationBuilder.DropIndex(
                name: "IX_Customers_PersonTypeId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PersonTypeId",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "LoanApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PersonType",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
