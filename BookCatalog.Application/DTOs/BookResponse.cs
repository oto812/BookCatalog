using BookCatalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookCatalog.Application.DTOs
{
    public record BookResponse(Guid Id,
    string Title,
    string Author,
    Genre genre);
}
