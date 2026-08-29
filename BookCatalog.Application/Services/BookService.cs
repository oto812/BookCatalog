using BookCatalog.Application.DTOs.Queries;
using BookCatalog.Application.DTOs.Requests;
using BookCatalog.Application.DTOs.Responses;
using BookCatalog.Application.Interfaces;
using BookCatalog.Application.Mappers;
using BookCatalog.Domain.Entities;
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
                createBookRequest.AuthorId,
                createBookRequest.PublicationYear,
                createBookRequest.Genre
            );
            var result = _bookRepository.AddBook(book);
            if(result == null)
            {
                return null;
            }
            _logger.LogInformation("Created book {BookId} by {Author}", book.Id, book.Author);

            return BookMapper.ToBookResponse(book);

        }

        public bool DeleteBook(Guid id)
        {
            var success = _bookRepository.DeleteBookById(id);
            if (success) _logger.LogInformation("Deleted book {BookId}", id);
            return success;

        }

        public PagedBooksResponse GetAllBooks(GetBooksQuery booksQuery)
        {
            var (books, totalBooks ) = _bookRepository.GetAll(booksQuery);
               
            var booksResponse = books.Select(book => BookMapper.ToBookResponse(book)).ToList();
            return new PagedBooksResponse(booksResponse, totalBooks);

        }

        

        public BookResponse? GetBookById(Guid id)
        {
            var book =  _bookRepository.GetById(id);
            if (book == null) {
                _logger.LogInformation("Get requested for unknown book {BookId}", id);
                return null; 
            }
            return BookMapper.ToBookResponse(book);
        }

        public BookResponse? UpdateBook(UpdateBookRequest updateBookDto, Guid id)
        {
            var oldBook = _bookRepository.GetById(id);

            if (oldBook == null) {
                _logger.LogInformation("Update requested for unknown book {BookId}", id);
                return null;
                }
            var newBook = oldBook.Update(updateBookDto.Title, updateBookDto.AuthorId, updateBookDto.PublicationYear, updateBookDto.Genre);

            var book = _bookRepository.UpdateBook(newBook, oldBook, id);
            
            if (book == null)
            {
                _logger.LogWarning("Concurrent update conflict for book {BookId}", id);
                return null;
            }
            _logger.LogInformation("Updated book {BookId}", id);
            return BookMapper.ToBookResponse(book);
        }
    }
}
