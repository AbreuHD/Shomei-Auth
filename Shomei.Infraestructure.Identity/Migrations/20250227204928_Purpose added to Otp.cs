using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Infraestructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class PurposeaddedtoOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpirationDate",
                table: "MailOtp",
                newName: "Expiration");

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "MailOtp",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "MailOtp");

            migrationBuilder.RenameColumn(
                name: "Expiration",
                table: "MailOtp",
                newName: "ExpirationDate");
        }
    }
}
