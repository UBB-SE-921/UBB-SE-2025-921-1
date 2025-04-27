using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class BuyerRepositoryMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StreetLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BuyerLinkages",
                columns: table => new
                {
                    RequestingBuyerId = table.Column<int>(type: "int", nullable: false),
                    ReceivingBuyerId = table.Column<int>(type: "int", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerLinkages", x => new { x.RequestingBuyerId, x.ReceivingBuyerId });
                    table.ForeignKey(
                        name: "FK_BuyerLinkages_Users_ReceivingBuyerId",
                        column: x => x.ReceivingBuyerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BuyerLinkages_Users_RequestingBuyerId",
                        column: x => x.RequestingBuyerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    SellerId = table.Column<int>(type: "int", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "Sellers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FollowersCount = table.Column<int>(type: "int", nullable: false),
                    StoreName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoreDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoreAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrustScore = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sellers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sellers_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuyerWishlistItems",
                columns: table => new
                {
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerWishlistItems", x => new { x.BuyerId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_BuyerWishlistItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BuyerWishlistItems_Users_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_BuyerLinkages_ReceivingBuyerId",
                table: "BuyerLinkages",
                column: "ReceivingBuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerWishlistItems_ProductId",
                table: "BuyerWishlistItems",
                column: "ProductId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Addresses_BillingAddressId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Addresses_ShippingAddressId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_BuyerUserId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "BuyerLinkages");

            migrationBuilder.DropTable(
                name: "BuyerWishlistItems");

            migrationBuilder.DropTable(
                name: "Sellers");

            migrationBuilder.DropTable(
                name: "Products");

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
        }
    }
}
