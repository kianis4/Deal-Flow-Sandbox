using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameCreditRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VendorTier",
                table: "Deals",
                newName: "CreditRating");

            migrationBuilder.AlterColumn<string>(
                name: "CreditRating",
                table: "Deals",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1)",
                oldMaxLength: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CreditRating",
                table: "Deals",
                type: "character varying(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3);

            migrationBuilder.RenameColumn(
                name: "CreditRating",
                table: "Deals",
                newName: "VendorTier");
        }
    }
}
