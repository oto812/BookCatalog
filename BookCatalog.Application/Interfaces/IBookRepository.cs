using BookCatalog.Application.DTOs;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Enums;

namespace BookCatalog.Application.Interfaces
{
    public interface IBookRepository
    {
        Book? GetById(Guid id);
        (IEnumerable<Book> Books, int TotalBooks) GetAll(string? author, Genre? genre, int? publicationYear, int page, int pageSize);
        Book? AddBook(Book book);
        Book? UpdateBook(Book newBook, Book oldBook, Guid id);
        bool DeleteBookById(Guid id);

    }
}
