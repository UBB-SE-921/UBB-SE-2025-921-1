using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class BuyerRepositoryFix1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Buyers_Buyers_BuyerId",
                table: "Buyers");

            migrationBuilder.DropIndex(
                name: "IX_Buyers_BuyerId",
                table: "Buyers");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "Buyers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BuyerId",
                table: "Buyers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Buyers_BuyerId",
                table: "Buyers",
                column: "BuyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Buyers_Buyers_BuyerId",
                table: "Buyers",
                column: "BuyerId",
                principalTable: "Buyers",
                principalColumn: "Id");
        }
    }
}
