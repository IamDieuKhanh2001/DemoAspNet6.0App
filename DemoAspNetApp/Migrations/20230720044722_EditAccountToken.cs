using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoAspNetApp.Migrations
{
    /// <inheritdoc />
    public partial class EditAccountToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Token",
                table: "AccountToken",
                newName: "RefreshToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "AccountToken",
                newName: "Token");
        }
    }
}
