
using BookCatalog.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BookCatalog.Application.DTOs.Queries
{
    public record GetBooksQuery(
        Guid? AuthorId, Genre? Genre, int? PublicationYear, [Range(1, int.MaxValue)] int Page = 1, [Range(1,100)] int PageSize = 10
        );
    
}
