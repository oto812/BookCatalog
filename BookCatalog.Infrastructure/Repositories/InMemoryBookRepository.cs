using BookCatalog.Application.DTOs.Queries;
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

        public (IEnumerable<Book> Books, int TotalBooks) GetAll(GetBooksQuery getBooksQuery)
        {
            var query = _bookRepository.Values.AsEnumerable();


            if (getBooksQuery.AuthorId != null)
            {
                query = query.Where(book => book.AuthorId == getBooksQuery.AuthorId);
            }

            if (getBooksQuery.Genre != null)
            {
                query = query.Where(book => book.Genre == getBooksQuery.Genre);
            }
            if(getBooksQuery.PublicationYear != null)
            {
                query = query.Where(book => book.PublicationYear == getBooksQuery.PublicationYear);
            }
            
            var books = query.OrderBy(book => book.CreatedAt)
                .Skip((getBooksQuery.Page - 1) * getBooksQuery.PageSize)
                .Take(getBooksQuery.PageSize).ToList();
            var total = query.Count();


            return (books, total);
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
