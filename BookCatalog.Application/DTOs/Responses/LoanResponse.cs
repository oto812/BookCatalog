
namespace BookCatalog.Application.DTOs.Responses
{
    public record LoanResponse(Guid LoanId, string BookTitle, DateTime BorrowedAt, DateTime? ReturnedAt);
}
