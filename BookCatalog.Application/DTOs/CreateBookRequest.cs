using BookCatalog.Domain.Enums;

namespace BookCatalog.Application.DTOs
{
    public record CreateBookRequest(
        string Title,
        string Author,
        int PublicationYear,
        Genre Genre
    );
}
