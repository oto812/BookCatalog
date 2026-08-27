using BookCatalog.Domain.Enums;

namespace BookCatalog.Application.DTOs.Responses
{
    public record BookResponse(
    Guid Id,
    string Title,
    string Author,
    Genre Genre,
    int PublicationYear);
}
