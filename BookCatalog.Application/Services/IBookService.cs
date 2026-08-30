using BookCatalog.Application.DTOs.Queries;
using BookCatalog.Application.DTOs.Requests;
using BookCatalog.Application.DTOs.Responses;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Enums;


namespace BookCatalog.Application.Services
{
    public interface IBookService
    {
        public Task<BookResponse?> AddBookAsync(CreateBookRequest book);
        public Task<BookResponse?> GetBookByIdAsync(Guid id);
        public Task<PagedBooksResponse> GetAllBooksAsync(GetBooksQuery getBooksQuery);
        public Task<BookResponse?> UpdateBookAsync(UpdateBookRequest updateBookDto, Guid id);
        public Task<bool> DeleteBookAsync(Guid id);
    }
}
