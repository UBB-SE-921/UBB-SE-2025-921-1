using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class ContractRepository0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "ContractID",
                table: "OrderNotifications",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "BuyerCartItems",
                columns: table => new
                {
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerCartItems", x => new { x.BuyerId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_BuyerCartItems_Buyers_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BuyerCartItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DummyCards",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardholderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CVC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Balance = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DummyCards", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DummyWallets",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Balance = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DummyWallets", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TrackedOrders",
                columns: table => new
                {
                    TrackedOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    CurrentStatus = table.Column<int>(type: "int", nullable: false),
                    EstimatedDeliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedOrders", x => x.TrackedOrderID);
                    table.CheckConstraint("TrackedOrderConstraint", "[CurrentStatus] IN ('PROCESSING', 'SHIPPED', 'IN_WAREHOUSE', 'IN_TRANSIT', 'OUT_FOR_DELIVERY', 'DELIVERED')");
                    table.ForeignKey(
                        name: "FK_TrackedOrders_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WaitlistProducts",
                columns: table => new
                {
                    WaitlistProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    AvailableAgain = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaitlistProducts", x => x.WaitlistProductID);
                    table.ForeignKey(
                        name: "FK_WaitlistProducts_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderCheckpoints",
                columns: table => new
                {
                    CheckpointID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TrackedOrderID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderCheckpoints", x => x.CheckpointID);
                    table.CheckConstraint("OrderChekpointConstraint", "[Status] IN ('PROCESSING', 'SHIPPED', 'IN_WAREHOUSE', 'IN_TRANSIT', 'OUT_FOR_DELIVERY', 'DELIVERED')");
                    table.ForeignKey(
                        name: "FK_OrderCheckpoints_TrackedOrders_TrackedOrderID",
                        column: x => x.TrackedOrderID,
                        principalTable: "TrackedOrders",
                        principalColumn: "TrackedOrderID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserWaitList",
                columns: table => new
                {
                    UserWaitListID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductWaitListID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    JoinedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PositionInQueue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWaitList", x => x.UserWaitListID);
                    table.ForeignKey(
                        name: "FK_UserWaitList_Buyers_UserID",
                        column: x => x.UserID,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserWaitList_WaitlistProducts_ProductWaitListID",
                        column: x => x.ProductWaitListID,
                        principalTable: "WaitlistProducts",
                        principalColumn: "WaitlistProductID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderNotifications_ContractID",
                table: "OrderNotifications",
                column: "ContractID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNotifications_OrderID",
                table: "OrderNotifications",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNotifications_ProductID",
                table: "OrderNotifications",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerCartItems_ProductId",
                table: "BuyerCartItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCheckpoints_TrackedOrderID",
                table: "OrderCheckpoints",
                column: "TrackedOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_TrackedOrders_OrderID",
                table: "TrackedOrders",
                column: "OrderID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserWaitList_ProductWaitListID",
                table: "UserWaitList",
                column: "ProductWaitListID");

            migrationBuilder.CreateIndex(
                name: "IX_UserWaitList_UserID",
                table: "UserWaitList",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistProducts_ProductID",
                table: "WaitlistProducts",
                column: "ProductID");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderNotifications_Contracts_ContractID",
                table: "OrderNotifications",
                column: "ContractID",
                principalTable: "Contracts",
                principalColumn: "ContractID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderNotifications_Orders_OrderID",
                table: "OrderNotifications",
                column: "OrderID",
                principalTable: "Orders",
                principalColumn: "OrderID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderNotifications_Products_ProductID",
                table: "OrderNotifications",
                column: "ProductID",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderNotifications_Contracts_ContractID",
                table: "OrderNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderNotifications_Orders_OrderID",
                table: "OrderNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderNotifications_Products_ProductID",
                table: "OrderNotifications");

            migrationBuilder.DropTable(
                name: "BuyerCartItems");

            migrationBuilder.DropTable(
                name: "DummyCards");

            migrationBuilder.DropTable(
                name: "DummyWallets");

            migrationBuilder.DropTable(
                name: "OrderCheckpoints");

            migrationBuilder.DropTable(
                name: "UserWaitList");

            migrationBuilder.DropTable(
                name: "TrackedOrders");

            migrationBuilder.DropTable(
                name: "WaitlistProducts");

            migrationBuilder.DropIndex(
                name: "IX_OrderNotifications_ContractID",
                table: "OrderNotifications");

            migrationBuilder.DropIndex(
                name: "IX_OrderNotifications_OrderID",
                table: "OrderNotifications");

            migrationBuilder.DropIndex(
                name: "IX_OrderNotifications_ProductID",
                table: "OrderNotifications");

            migrationBuilder.AlterColumn<int>(
                name: "ContractID",
                table: "OrderNotifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
