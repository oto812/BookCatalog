using BookCatalog.Application.DTOs.Requests;
using BookCatalog.Application.DTOs.Responses;
using BookCatalog.Application.Results;

namespace BookCatalog.Application.Services
{
    public interface ILoanService
    {
        Task<BorrowBookResult> BorrowBookAsync(BorrowBookRequest borrowBookRequest, CancellationToken cancellationToken);
        Task<ReturnBookResult> ReturnBookAsync(Guid loanId, CancellationToken cancellationToken);
        Task<IEnumerable<LoanResponse>> LoanHistoryAsync(Guid userId, CancellationToken cancellationToken);
    }
}
