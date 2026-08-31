using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BookCatalog.Application.DTOs.Requests
{
    public record BorrowBookRequest([Required] Guid UserId, [Required] Guid BookId);
}
