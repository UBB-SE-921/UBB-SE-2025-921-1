using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class ServiceRepository5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PDFs",
                columns: table => new
                {
                    PdfID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    File = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDFs", x => x.PdfID);
                });

            migrationBuilder.CreateTable(
                name: "PredefinedContracts",
                columns: table => new
                {
                    ContractID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractContent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredefinedContracts", x => x.ContractID);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    ContractID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    ContractStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContractContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RenewalCount = table.Column<int>(type: "int", nullable: false),
                    PredefinedContractID = table.Column<int>(type: "int", nullable: true),
                    PDFID = table.Column<int>(type: "int", nullable: false),
                    AdditionalTerms = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RenewedFromContractID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.ContractID);
                    table.CheckConstraint("ContractStatusConstraint", "[ContractStatus] IN ('ACTIVE', 'RENEWED', 'EXPIRED')");
                    table.ForeignKey(
                        name: "FK_Contracts_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_PDFs_PDFID",
                        column: x => x.PDFID,
                        principalTable: "PDFs",
                        principalColumn: "PdfID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_PredefinedContracts_PredefinedContractID",
                        column: x => x.PredefinedContractID,
                        principalTable: "PredefinedContracts",
                        principalColumn: "ContractID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_OrderID",
                table: "Contracts",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_PDFID",
                table: "Contracts",
                column: "PDFID");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_PredefinedContractID",
                table: "Contracts",
                column: "PredefinedContractID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "PDFs");

            migrationBuilder.DropTable(
                name: "PredefinedContracts");
        }
    }
}
