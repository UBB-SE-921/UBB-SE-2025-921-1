using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class DummyWalletRepository0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BuyerId",
                table: "DummyWallets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DummyWallets_BuyerId",
                table: "DummyWallets",
                column: "BuyerId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DummyWallets_Buyers_BuyerId",
                table: "DummyWallets",
                column: "BuyerId",
                principalTable: "Buyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DummyWallets_Buyers_BuyerId",
                table: "DummyWallets");

            migrationBuilder.DropIndex(
                name: "IX_DummyWallets_BuyerId",
                table: "DummyWallets");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "DummyWallets");
        }
    }
}
