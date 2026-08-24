using System;
using System.Collections.Generic;
using System.Text;

namespace BookCatalog.Application.DTOs
{
    public record PagedBooksResponse(IEnumerable<BookResponse> Books, int TotalBooks);
}
