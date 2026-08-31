using BookCatalog.Application.DTOs.Responses;


namespace BookCatalog.Application.Results
{
    public enum ReturnBookOutcome
    {
        Success,
        LoanNotFound,
        AlreadyReturned,
    }
    public record ReturnBookResult(ReturnBookOutcome Outcome, LoanResponse? Loan);

}
