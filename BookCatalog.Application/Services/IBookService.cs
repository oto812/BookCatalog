using BookCatalog.Application.DTOs;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Enums;


namespace BookCatalog.Application.Services
{
    public interface IBookService
    {
        public BookResponse? AddBook(CreateBookRequest book);
        public BookResponse? GetBookById(Guid id);
        public PagedBooksResponse GetAllBooks(string? author, Genre? genre, int? publicationYear, int page, int pageSize);
        public BookResponse? UpdateBook(UpdateBookRequest updateBookDto, Guid id);
        public bool DeleteBook(Guid id);
    }
}
