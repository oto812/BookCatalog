using BookCatalog.Domain.Enums;

namespace BookCatalog.Application.DTOs
{
    public record BookResponse(Guid Id,
    string Title,
    string Author,
    Genre Genre);
}
