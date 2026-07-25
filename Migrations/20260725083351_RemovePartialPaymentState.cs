using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_do_an1.Migrations
{
    /// <inheritdoc />
    public partial class RemovePartialPaymentState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Payments
                SET Status = 'Unpaid',
                    PaidAmount = 0,
                    PaidDate = NULL
                WHERE Status = 'PartiallyPaid';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
