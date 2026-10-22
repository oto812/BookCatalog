using BookCatalog.Application.DTOs;
using BookCatalog.Application.DTOs.Queries;
using BookCatalog.Application.DTOs.Requests;
using BookCatalog.Application.Idempotency;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Enums;

namespace BookCatalog.Application.Interfaces
{
    public interface IBookRepository
    {
        Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<(IEnumerable<Book> Books, int TotalBooks)> GetAllAsync(GetBooksQuery getBooksQuery, CancellationToken cancellationToken);
        Task<Book> AddAsync(Book book, CancellationToken cancellationToken);
        // Saves both in one transaction. False when the key is already taken - nothing is saved then.
        Task<bool> AddWithIdempotencyKeyAsync(Book book, IdempotencyRecord record, CancellationToken cancellationToken);
        Task<IdempotencyRecord?> GetIdempotencyRecordAsync(string key, CancellationToken cancellationToken);
        Task UpdateAsync(Book book, CancellationToken cancellationToken);
        Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken);

    }
}
