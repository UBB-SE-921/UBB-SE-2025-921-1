using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class BuyerRepositoryFix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Buyers");

            migrationBuilder.DropColumn(
                name: "FollowingUsersIds",
                table: "Buyers");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Buyers");

            migrationBuilder.CreateTable(
                name: "Followings",
                columns: table => new
                {
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    SellerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Followings", x => new { x.BuyerId, x.SellerId });
                    table.ForeignKey(
                        name: "FK_Followings_Buyers_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Followings_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Followings_SellerId",
                table: "Followings",
                column: "SellerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Followings");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Buyers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FollowingUsersIds",
                table: "Buyers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Buyers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
