using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class BuyerRepositoryFix4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_BuyerLinkage_DifferentBuyers",
                table: "BuyerLinkages",
                sql: "[RequestingBuyerId] <> [ReceivingBuyerId]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BuyerLinkage_DifferentBuyers",
                table: "BuyerLinkages");
        }
    }
}
