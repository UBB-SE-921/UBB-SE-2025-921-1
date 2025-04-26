using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class BuyerRepositoryFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuyerLinkages_Users_ReceivingBuyerId",
                table: "BuyerLinkages");

            migrationBuilder.DropForeignKey(
                name: "FK_BuyerLinkages_Users_RequestingBuyerId",
                table: "BuyerLinkages");

            migrationBuilder.DropForeignKey(
                name: "FK_BuyerWishlistItems_Users_BuyerId",
                table: "BuyerWishlistItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Addresses_BillingAddressId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Addresses_ShippingAddressId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_BuyerUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_BillingAddressId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_BuyerUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ShippingAddressId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Badge",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BillingAddressId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BuyerUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FollowingUsersIds",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NumberOfPurchases",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShippingAddressId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotalSpending",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UseSameAddress",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "Buyers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UseSameAddress = table.Column<bool>(type: "bit", nullable: false),
                    Badge = table.Column<int>(type: "int", nullable: false),
                    TotalSpending = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NumberOfPurchases = table.Column<int>(type: "int", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingAddressId = table.Column<int>(type: "int", nullable: false),
                    BillingAddressId = table.Column<int>(type: "int", nullable: false),
                    FollowingUsersIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BuyerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buyers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Buyers_Addresses_BillingAddressId",
                        column: x => x.BillingAddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Buyers_Addresses_ShippingAddressId",
                        column: x => x.ShippingAddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Buyers_Buyers_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Buyers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Buyers_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buyers_BillingAddressId",
                table: "Buyers",
                column: "BillingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Buyers_BuyerId",
                table: "Buyers",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Buyers_ShippingAddressId",
                table: "Buyers",
                column: "ShippingAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_BuyerLinkages_Buyers_ReceivingBuyerId",
                table: "BuyerLinkages",
                column: "ReceivingBuyerId",
                principalTable: "Buyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BuyerLinkages_Buyers_RequestingBuyerId",
                table: "BuyerLinkages",
                column: "RequestingBuyerId",
                principalTable: "Buyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BuyerWishlistItems_Buyers_BuyerId",
                table: "BuyerWishlistItems",
                column: "BuyerId",
                principalTable: "Buyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuyerLinkages_Buyers_ReceivingBuyerId",
                table: "BuyerLinkages");

            migrationBuilder.DropForeignKey(
                name: "FK_BuyerLinkages_Buyers_RequestingBuyerId",
                table: "BuyerLinkages");

            migrationBuilder.DropForeignKey(
                name: "FK_BuyerWishlistItems_Buyers_BuyerId",
                table: "BuyerWishlistItems");

            migrationBuilder.DropTable(
                name: "Buyers");

            migrationBuilder.AddColumn<int>(
                name: "Badge",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BillingAddressId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuyerUserId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "Users",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Users",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FollowingUsersIds",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfPurchases",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShippingAddressId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSpending",
                table: "Users",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseSameAddress",
                table: "Users",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_BillingAddressId",
                table: "Users",
                column: "BillingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_BuyerUserId",
                table: "Users",
                column: "BuyerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ShippingAddressId",
                table: "Users",
                column: "ShippingAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_BuyerLinkages_Users_ReceivingBuyerId",
                table: "BuyerLinkages",
                column: "ReceivingBuyerId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BuyerLinkages_Users_RequestingBuyerId",
                table: "BuyerLinkages",
                column: "RequestingBuyerId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BuyerWishlistItems_Users_BuyerId",
                table: "BuyerWishlistItems",
                column: "BuyerId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Addresses_BillingAddressId",
                table: "Users",
                column: "BillingAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Addresses_ShippingAddressId",
                table: "Users",
                column: "ShippingAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_BuyerUserId",
                table: "Users",
                column: "BuyerUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
