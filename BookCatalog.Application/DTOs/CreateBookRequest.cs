using System;
using System.Collections.Generic;
using System.Text;

namespace BookCatalog.Application.DTOs
{
    public record CreateBookRequest(
        string Title,
        string Author,
        int PublicationYear,
        string Genre
    );
}
