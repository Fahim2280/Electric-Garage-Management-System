using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGMS.Migrations
{
    /// <inheritdoc />
    public partial class Init5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElectricBills",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Customer_ID = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Previous_unit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total_Unit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Electric_bill = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Previous_duos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rent_Bill = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total_bill = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Clear_money = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Present_dues = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectricBills", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ElectricBills_Customers_Customer_ID",
                        column: x => x.Customer_ID,
                        principalTable: "Customers",
                        principalColumn: "C_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ElectricBills_Customer_ID",
                table: "ElectricBills",
                column: "Customer_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElectricBills");
        }
    }
}
