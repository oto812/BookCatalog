using BookCatalog.Application.DTOs;
using BookCatalog.Application.Interfaces;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Enums;
using Microsoft.Extensions.Logging;


namespace BookCatalog.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<BookService> _logger;
        public BookService(IBookRepository bookRepository, ILogger<BookService> logger)
        {
            _bookRepository = bookRepository;
            _logger = logger;
        }

        public BookResponse? AddBook(CreateBookRequest createBookRequest)
        {
            var book = new Book(
                createBookRequest.Title,
                createBookRequest.Author,
                createBookRequest.PublicationYear,
                createBookRequest.Genre
            );
            var result = _bookRepository.AddBook(book);
            if(result == null)
            {
                return null;
            }
            _logger.LogInformation("Created book {BookId} by {Author}", book.Id, book.Author);

            return new BookResponse(
                book.Id,
                book.Title,
                book.Author,
                book.Genre,
                book.PublicationYear
            );

        }

        public bool DeleteBook(Guid id)
        {
            var success = _bookRepository.DeleteBookById(id);
            if (success) _logger.LogInformation("Deleted book {BookId}", id);
            return success;

        }

        public IEnumerable<BookResponse> GetAllBooks(string? author, Genre? genre, int? publicationYear)
        {
               var books = _bookRepository.GetAll(author, genre, publicationYear);
               return books.Select(book => new BookResponse
               (
                   book.Id,
                   book.Title,
                   book.Author,
                   book.Genre,
                   book.PublicationYear
               ));
        }

        

        public BookResponse? GetBookById(Guid id)
        {
            var book =  _bookRepository.GetById(id);
            if (book == null) {
                _logger.LogInformation("Get requested for unknown book {BookId}", id);
                return null; 
            }
            return new BookResponse
            (
                book.Id,
                book.Title,
                book.Author,
                book.Genre,
                book.PublicationYear
            );
        }

        public BookResponse? UpdateBook(UpdateBookRequest updateBookDto, Guid id)
        {
            var oldBook = _bookRepository.GetById(id);

            if (oldBook == null) {
                _logger.LogInformation("Update requested for unknown book {BookId}", id);
                return null;
                }
            var newBook = oldBook.Update(updateBookDto.Title, updateBookDto.Author, updateBookDto.PublicationYear, updateBookDto.Genre);

            var book = _bookRepository.UpdateBook(newBook, oldBook, id);
            
            if (book == null)
            {
                _logger.LogWarning("Concurrent update conflict for book {BookId}", id);
                return null;
            }
            _logger.LogInformation("Updated book {BookId}", id);
            return new BookResponse ( id, book.Title, book.Author, book.Genre, book.PublicationYear );
        }
    }
}
