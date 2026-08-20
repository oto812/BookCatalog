using BookCatalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookCatalog.Application.DTOs
{
    public record UpdateBookRequest(
        string Title,
        string Author,
        int PublicationYear,
        Genre Genre
    );
}
