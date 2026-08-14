using BookCatalog.Domain.Entities;

namespace BookCatalog.Application
{
    public interface IBookRepository
    {
        Task<Book> GetByIdAsync(Guid id);
        Task<IEnumerable<Book>> GetAllAsync();
        Task AddAsync(Book book);
    }
}
