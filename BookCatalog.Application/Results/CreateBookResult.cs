using BookCatalog.Application.DTOs.Responses;


namespace BookCatalog.Application.Results
{

    public enum CreateBookOutcome
    {
        Created,
        Replayed,
        KeyReusedWithDifferentRequest,
    }

    public record CreateBookResult(CreateBookOutcome Outcome, BookResponse? Book);
}
