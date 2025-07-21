using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSysAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFullNameFromUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Accomodation",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Citizenship",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TaxOffice",
                table: "Customers");

            migrationBuilder.AlterColumn<int>(
                name: "CitizenshipCountryId",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AccomodationCountryId",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccomodationId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CitizenshipId",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LanguageId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaxOfficeId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Accomodations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accomodations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Citizenships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citizenships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxOffices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxOffices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AccomodationId",
                table: "Customers",
                column: "AccomodationId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CitizenshipId",
                table: "Customers",
                column: "CitizenshipId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_LanguageId",
                table: "Customers",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TaxOfficeId",
                table: "Customers",
                column: "TaxOfficeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Accomodations_AccomodationId",
                table: "Customers",
                column: "AccomodationId",
                principalTable: "Accomodations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Citizenships_CitizenshipId",
                table: "Customers",
                column: "CitizenshipId",
                principalTable: "Citizenships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Languages_LanguageId",
                table: "Customers",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_TaxOffices_TaxOfficeId",
                table: "Customers",
                column: "TaxOfficeId",
                principalTable: "TaxOffices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Accomodations_AccomodationId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Citizenships_CitizenshipId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Languages_LanguageId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_TaxOffices_TaxOfficeId",
                table: "Customers");

            migrationBuilder.DropTable(
                name: "Accomodations");

            migrationBuilder.DropTable(
                name: "Citizenships");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "TaxOffices");

            migrationBuilder.DropIndex(
                name: "IX_Customers_AccomodationId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CitizenshipId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_LanguageId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_TaxOfficeId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AccomodationId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CitizenshipId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "LanguageId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TaxOfficeId",
                table: "Customers");

            migrationBuilder.AlterColumn<int>(
                name: "CitizenshipCountryId",
                table: "Customers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AccomodationCountryId",
                table: "Customers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Accomodation",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Citizenship",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxOffice",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
