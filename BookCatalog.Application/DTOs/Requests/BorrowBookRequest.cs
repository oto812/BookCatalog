using System.ComponentModel.DataAnnotations;


namespace BookCatalog.Application.DTOs.Requests
{
    public record BorrowBookRequest([Required] Guid UserId, [Required] Guid BookId);
}
