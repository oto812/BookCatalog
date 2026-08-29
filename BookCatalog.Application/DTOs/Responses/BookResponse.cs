using BookCatalog.Domain.Enums;

namespace BookCatalog.Application.DTOs.Responses
{
    public record BookResponse(
    Guid Id,
    string Title,
    Guid AuthorId,
    Genre Genre,
    int PublicationYear);
}
