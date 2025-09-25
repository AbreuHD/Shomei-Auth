using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Infraestructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class UserS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserSession",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "UserSession",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "UserSession",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserSession");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "UserSession");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "UserSession");
        }
    }
}
