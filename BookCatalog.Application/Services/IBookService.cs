using BookCatalog.Application.DTOs;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Enums;


namespace BookCatalog.Application.Services
{
    public interface IBookService
    {
        public BookResponse? AddBook(CreateBookRequest book);
        public BookResponse? GetBookById(Guid id);
        public IEnumerable<BookResponse> GetAllBooks(string? author, Genre? genre, int? publicationYear);
        public BookResponse? UpdateBook(UpdateBookRequest updateBookDto, Guid id);
        public bool DeleteBook(Guid id);
    }
}
