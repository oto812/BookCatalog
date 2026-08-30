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

        public async Task<BookResponse?> AddBookAsync(CreateBookRequest createBookRequest)
        {
            var book = new Book(
                createBookRequest.Title,
                createBookRequest.AuthorId,
                createBookRequest.PublicationYear,
                createBookRequest.Genre
            );
            var result = await _bookRepository.AddAsync(book);
            if(result == null)
            {
                return null;
            }
            _logger.LogInformation("Created book {BookId} by {Author}", book.Id, book.Author);

            return BookMapper.ToBookResponse(book);

        }

        public async Task<bool> DeleteBookAsync(Guid id)
        {
            var success = await _bookRepository.DeleteByIdAsync(id);
            if (success) _logger.LogInformation("Deleted book {BookId}", id);
            return success;

        }

        public async Task<PagedBooksResponse> GetAllBooksAsync(GetBooksQuery booksQuery)
        {
            var (books, totalBooks ) = await _bookRepository.GetAllAsync(booksQuery);
               
            var booksResponse = books.Select(book => BookMapper.ToBookResponse(book)).ToList();
            return new PagedBooksResponse(booksResponse, totalBooks);

        }

        

        public async Task<BookResponse?> GetBookByIdAsync(Guid id)
        {
            var book =  await _bookRepository.GetByIdAsync(id);
            if (book == null) {
                _logger.LogInformation("Get requested for unknown book {BookId}", id);
                return null; 
            }
            return BookMapper.ToBookResponse(book);
        }

        public async Task<BookResponse?> UpdateBookAsync(UpdateBookRequest updateBookDto, Guid id)
        {
            
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) {
                _logger.LogInformation("Update requested for unknown book {BookId}", id);
                return null;
                }

            book.Update(updateBookDto.Title, updateBookDto.AuthorId, updateBookDto.PublicationYear, updateBookDto.Genre);
            await _bookRepository.UpdateAsync(book);
            _logger.LogInformation("Updated book {BookId}", id);

            return BookMapper.ToBookResponse(book);
        }
    }
}
