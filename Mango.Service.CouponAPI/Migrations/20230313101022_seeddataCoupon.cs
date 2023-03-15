using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mango.Service.CouponAPI.Migrations
{
    /// <inheritdoc />
    public partial class seeddataCoupon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Coupons",
                columns: new[] { "CounponId", "CounponCode", "DiscountAmount" },
                values: new object[,]
                {
                    { 1, "10OFF", 10.0 },
                    { 2, "20OFF", 20.0 },
                    { 3, "30OFF", 30.0 },
                    { 4, "40OFF", 40.0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "CounponId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "CounponId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "CounponId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "CounponId",
                keyValue: 4);
        }
    }
}
