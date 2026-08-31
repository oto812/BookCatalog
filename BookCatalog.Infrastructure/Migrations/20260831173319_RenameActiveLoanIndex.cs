using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookCatalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameActiveLoanIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Loans_BookId",
                table: "Loans",
                newName: "UX_Loan_ActiveBook");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "UX_Loan_ActiveBook",
                table: "Loans",
                newName: "IX_Loans_BookId");
        }
    }
}
