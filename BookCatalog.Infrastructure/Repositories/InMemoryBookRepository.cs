using BookCatalog.Application;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Enums;
using System.Collections.Concurrent;


namespace BookCatalog.Infrastructure.Repositories
{
    public class InMemoryBookRepository : IBookRepository
    {
        private readonly ConcurrentDictionary<Guid, Book> _bookRepository;
        public InMemoryBookRepository()
        {
            _bookRepository = new ConcurrentDictionary<Guid, Book>();
        }
        public Task AddAsync(Book book)
        {
            _bookRepository.TryAdd(book.Id, book);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Book>> GetAllAsync()
        {
            var books = _bookRepository.Values.ToList();
            return Task.FromResult<IEnumerable<Book>>(books);
        }

        public Task<Book?> GetByIdAsync(Guid id)
        {
            return Task.FromResult(_bookRepository.GetValueOrDefault(id));
        }
    }
}
