using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_do_an1.Migrations
{
    /// <inheritdoc />
    public partial class RenamePasswordColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "UserAccounts",
                newName: "Password");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "UserAccounts",
                newName: "PasswordHash");
        }
    }
}
