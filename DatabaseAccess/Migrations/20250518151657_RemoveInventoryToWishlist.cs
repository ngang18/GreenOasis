using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInventoryToWishlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wishlists_Inventories_inventoryId",
                table: "Wishlists");

            migrationBuilder.DropIndex(
                name: "IX_Wishlists_inventoryId",
                table: "Wishlists");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Wishlists");

            migrationBuilder.DropColumn(
                name: "inventoryId",
                table: "Wishlists");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "Wishlists",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "inventoryId",
                table: "Wishlists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Wishlists_inventoryId",
                table: "Wishlists",
                column: "inventoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Wishlists_Inventories_inventoryId",
                table: "Wishlists",
                column: "inventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
