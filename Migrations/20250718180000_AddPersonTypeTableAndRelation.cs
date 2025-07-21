using Microsoft.EntityFrameworkCore.Migrations;

namespace BankSysAPI.Migrations
{
    public partial class AddPersonTypeTableAndRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PersonTypes",
                columns: new[] { "Name" },
                values: new object[] { "Bireysel" });
            migrationBuilder.InsertData(
                table: "PersonTypes",
                columns: new[] { "Name" },
                values: new object[] { "Kurumsal" });

            migrationBuilder.AddColumn<int>(
                name: "PersonTypeId",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 1 // Default to 'Bireysel' for existing rows
            );

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
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_PersonTypes_PersonTypeId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_PersonTypeId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PersonTypeId",
                table: "Customers");

            migrationBuilder.DropTable(
                name: "PersonTypes");
        }
    }
} 