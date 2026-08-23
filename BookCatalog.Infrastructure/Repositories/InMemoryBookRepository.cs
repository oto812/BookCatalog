using BookCatalog.Application.Interfaces;
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
        public Book? AddBook(Book book)
        {
            if (_bookRepository.TryAdd(book.Id, book))
            {
                return book;
            }
            return null;
        }

        public bool DeleteBookById(Guid id)
        {
            return _bookRepository.TryRemove(id, out _);
        }

        public IEnumerable<Book> GetAll(string? author, Genre? genre, int? publicationYear)
        {
            var query = _bookRepository.Values.AsEnumerable();

            if (author != null)
            {
                query = query.Where(book => book.Author == author);
            }

            if (genre != null)
            {
                query = query.Where(book => book.Genre == genre);
            }
            if(publicationYear != null)
            {
                query = query.Where(book => book.PublicationYear == publicationYear);
            }

            return query;
        }

        public Book? GetById(Guid id)
        {
            return _bookRepository.GetValueOrDefault(id);
        }

        public Book? UpdateBook(Book newBook, Book oldBook, Guid id)
        {

            if(_bookRepository.TryUpdate(id, newBook, oldBook)) return newBook;
            return null;
        }
    }
}
