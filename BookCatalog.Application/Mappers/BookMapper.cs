using BookCatalog.Application.DTOs.Responses;
using BookCatalog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BookCatalog.Application.Mappers
{
    public static class BookMapper
    {
        public static BookResponse ToBookResponse(Book book)
        {
            return new BookResponse(
                book.Id,
                book.Title,
                book.Author,
                book.Genre,
                book.PublicationYear
            );
        }
    }
}
