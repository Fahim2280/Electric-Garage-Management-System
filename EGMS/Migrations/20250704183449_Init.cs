using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGMS.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    C_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    F_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    M_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mobile_number = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    NID_Number = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: false),
                    Created_Date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Previous_Unit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Advance_money = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.C_ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElectricBills",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Customer_ID = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Previous_unit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Current_Unit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                name: "IX_Customers_Mobile_number",
                table: "Customers",
                column: "Mobile_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_NID_Number",
                table: "Customers",
                column: "NID_Number",
                unique: true);

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

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
