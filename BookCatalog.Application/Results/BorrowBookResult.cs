using BookCatalog.Application.DTOs.Responses;


namespace BookCatalog.Application.Results
{
    
    public enum BorrowOutcome
    {
        BookNotFound,
        BookAlreadyBorrowed,
        Success,
    }

    public record BorrowBookResult(BorrowOutcome Outcome, LoanResponse? Loan);
}
