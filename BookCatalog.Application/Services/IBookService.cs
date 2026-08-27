using BookCatalog.Application.DTOs.Queries;
using BookCatalog.Application.DTOs.Requests;
using BookCatalog.Application.DTOs.Responses;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Enums;


namespace BookCatalog.Application.Services
{
    public interface IBookService
    {
        public BookResponse? AddBook(CreateBookRequest book);
        public BookResponse? GetBookById(Guid id);
        public PagedBooksResponse GetAllBooks(GetBooksQuery getBooksQuery);
        public BookResponse? UpdateBook(UpdateBookRequest updateBookDto, Guid id);
        public bool DeleteBook(Guid id);
    }
}
