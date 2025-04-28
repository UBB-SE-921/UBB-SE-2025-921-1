using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class ServiceRepository1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "NotificationCategoryConstraint",
                table: "OrderNotifications",
                sql: "[Category] IN ('CONTRACT_EXPIRATION', 'OUTBIDDED', 'ORDER_SHIPPING_PROGRESS', 'PRODUCT_AVAILABLE', 'PAYMENT_CONFIRMATION', 'PRODUCT_REMOVED', 'CONTRACT_RENEWAL_REQ', 'CONTRACT_RENEWAL_ANS', 'CONTRACT_RENEWAL_WAITLIST')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "NotificationCategoryConstraint",
                table: "OrderNotifications");
        }
    }
}
