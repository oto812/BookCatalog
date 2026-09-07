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

        public async Task<BookResponse?> AddBookAsync(CreateBookRequest createBookRequest, CancellationToken cancellationToken)
        {
            var book = new Book(
                createBookRequest.Title,
                createBookRequest.AuthorId,
                createBookRequest.PublicationYear,
                createBookRequest.Genre
            );
            await _bookRepository.AddAsync(book, cancellationToken);
            
            _logger.LogInformation("Created book {BookId} by {AuthorId}", book.Id, book.AuthorId);

            return BookMapper.ToBookResponse(book);

        }

        public async Task<bool> DeleteBookAsync(Guid id, CancellationToken cancellationToken)
        {

            var success = await _bookRepository.DeleteByIdAsync(id, cancellationToken);
            if (success) _logger.LogInformation("Deleted book {BookId}", id);
            return success;

        }

        public async Task<PagedBooksResponse> GetAllBooksAsync(GetBooksQuery booksQuery, CancellationToken cancellationToken)
        {
            var (books, totalBooks) = await _bookRepository.GetAllAsync(booksQuery, cancellationToken);
               
            var booksResponse = books.Select(book => BookMapper.ToBookResponse(book)).ToList();
            return new PagedBooksResponse(booksResponse, totalBooks);

        }

        

        public async Task<BookResponse?> GetBookByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var book =  await _bookRepository.GetByIdAsync(id, cancellationToken);
            if (book == null) {
                return null; 
            }
            return BookMapper.ToBookResponse(book);
        }

        public async Task<BookResponse?> UpdateBookAsync(UpdateBookRequest updateBookDto, Guid id, CancellationToken cancellationToken)
        {
            
            var book = await _bookRepository.GetByIdAsync(id, cancellationToken);
            if (book == null) {
                return null;
                }

            book.Update(updateBookDto.Title, updateBookDto.AuthorId, updateBookDto.PublicationYear, updateBookDto.Genre);
            await _bookRepository.UpdateAsync(book, cancellationToken);
            _logger.LogInformation("Updated book {BookId}", id);

            return BookMapper.ToBookResponse(book);
        }
    }
}
