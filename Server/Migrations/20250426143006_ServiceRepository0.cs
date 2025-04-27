using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class ServiceRepository0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderNotifications",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientID = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ContractID = table.Column<int>(type: "int", nullable: true),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: true),
                    ProductID = table.Column<int>(type: "int", nullable: true),
                    OrderID = table.Column<int>(type: "int", nullable: true),
                    ShippingState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderNotifications", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK_OrderNotifications_Buyers_RecipientID",
                        column: x => x.RecipientID,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SellerNotifications",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellerID = table.Column<int>(type: "int", nullable: false),
                    NotificationMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotificationFollowerCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerNotifications", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK_SellerNotifications_Sellers_SellerID",
                        column: x => x.SellerID,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderNotifications_RecipientID",
                table: "OrderNotifications",
                column: "RecipientID");

            migrationBuilder.CreateIndex(
                name: "IX_SellerNotifications_SellerID",
                table: "SellerNotifications",
                column: "SellerID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderNotifications");

            migrationBuilder.DropTable(
                name: "SellerNotifications");
        }
    }
}
