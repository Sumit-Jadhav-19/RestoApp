using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestoApp.Migrations
{
    /// <inheritdoc />
    public partial class addPaymentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBillOrder",
                table: "OrderMasters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBillPayed",
                table: "OrderMasters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentType",
                table: "OrderMasters",
                type: "nvarchar(50)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBillOrder",
                table: "OrderMasters");

            migrationBuilder.DropColumn(
                name: "IsBillPayed",
                table: "OrderMasters");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "OrderMasters");
        }
    }
}
