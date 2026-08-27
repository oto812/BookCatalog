using BookCatalog.Application.Validation;
using BookCatalog.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BookCatalog.Application.DTOs.Requests
{
    public record UpdateBookRequest(
        [Required]
        [MinLength(1, ErrorMessage = "Title must be at least 1 character long.")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        string Title,

        [Required]
        [MinLength(1, ErrorMessage = "Author must be at least 1 character long.")]
        [MaxLength(100, ErrorMessage = "Author cannot exceed 100 characters.")]
        string Author,

        
        [Range(0, 2026, ErrorMessage = "Publication year cannot be negative or in the future.")]
        int PublicationYear,

        
        [EnumValue<Genre>(ErrorMessage = "Invalid genre value.")]
        Genre Genre
    );
}
