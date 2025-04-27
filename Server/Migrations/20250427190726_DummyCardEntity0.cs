using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class DummyCardEntity0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DummyWallets_Buyers_BuyerId",
                table: "DummyWallets");

            migrationBuilder.AddColumn<int>(
                name: "BuyerId",
                table: "DummyCards",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DummyCards_BuyerId",
                table: "DummyCards",
                column: "BuyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_DummyCards_Buyers_BuyerId",
                table: "DummyCards",
                column: "BuyerId",
                principalTable: "Buyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DummyWallets_Buyers_BuyerId",
                table: "DummyWallets",
                column: "BuyerId",
                principalTable: "Buyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DummyCards_Buyers_BuyerId",
                table: "DummyCards");

            migrationBuilder.DropForeignKey(
                name: "FK_DummyWallets_Buyers_BuyerId",
                table: "DummyWallets");

            migrationBuilder.DropIndex(
                name: "IX_DummyCards_BuyerId",
                table: "DummyCards");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "DummyCards");

            migrationBuilder.AddForeignKey(
                name: "FK_DummyWallets_Buyers_BuyerId",
                table: "DummyWallets",
                column: "BuyerId",
                principalTable: "Buyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
