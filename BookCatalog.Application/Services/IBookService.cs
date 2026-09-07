using BookCatalog.Application.DTOs.Queries;
using BookCatalog.Application.DTOs.Requests;
using BookCatalog.Application.DTOs.Responses;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Enums;


namespace BookCatalog.Application.Services
{
    public interface IBookService
    {
        public Task<BookResponse?> AddBookAsync(CreateBookRequest book, CancellationToken cancellationToken);
        public Task<BookResponse?> GetBookByIdAsync(Guid id, CancellationToken cancellationToken);
        public Task<PagedBooksResponse> GetAllBooksAsync(GetBooksQuery getBooksQuery, CancellationToken cancellationToken);
        public Task<BookResponse?> UpdateBookAsync(UpdateBookRequest updateBookDto, Guid id, CancellationToken cancellationToken);
        public Task<bool> DeleteBookAsync(Guid id, CancellationToken cancellationToken);
    }
}
