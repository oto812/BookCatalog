using BookCatalog.Application.Validation;
using BookCatalog.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BookCatalog.Application.DTOs.Requests
{
    public record CreateBookRequest(
        [Required]
        [MinLength(1, ErrorMessage = "Title must be at least 1 character long.")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        string Title,

        [Required]
        Guid AuthorId,

        [Required]
        [PublicationYear]
        int PublicationYear,

        [Required]
        [EnumValue<Genre>(ErrorMessage = "Invalid genre value.")]
        Genre Genre
    );
}
