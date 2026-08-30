using BookCatalog.Application.DTOs;
using BookCatalog.Application.DTOs.Queries;
using BookCatalog.Application.DTOs.Requests;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Enums;

namespace BookCatalog.Application.Interfaces
{
    public interface IBookRepository
    {
        Task<Book?> GetByIdAsync(Guid id);
        Task<(IEnumerable<Book> Books, int TotalBooks)> GetAllAsync(GetBooksQuery getBooksQuery);
        Task<Book> AddAsync(Book book);
        Task UpdateAsync(Book book);
        Task<bool> DeleteByIdAsync(Guid id);

    }
}
