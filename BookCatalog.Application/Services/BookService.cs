using BookCatalog.Application.DTOs.Queries;
using BookCatalog.Application.DTOs.Requests;
using BookCatalog.Application.DTOs.Responses;
using BookCatalog.Application.Idempotency;
using BookCatalog.Application.Interfaces;
using BookCatalog.Application.Mappers;
using BookCatalog.Application.Results;
using BookCatalog.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text.Json;


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

        public async Task<CreateBookResult> AddBookAsync(CreateBookRequest createBookRequest, string? idempotencyKey, CancellationToken cancellationToken)
        {
            var book = new Book(
                createBookRequest.Title,
                createBookRequest.AuthorId,
                createBookRequest.PublicationYear,
                createBookRequest.Genre
            );

            if (idempotencyKey == null)
            {
                await _bookRepository.AddAsync(book, cancellationToken);
                _logger.LogInformation("Created book {BookId} {Title} by author {AuthorId}",
                    book.Id, book.Title, book.AuthorId);
                return new CreateBookResult(CreateBookOutcome.Created, BookMapper.ToBookResponse(book));
            }

            var requestHash = HashRequest(createBookRequest);
            var record = new IdempotencyRecord(idempotencyKey, requestHash, book.Id);

            if (await _bookRepository.AddWithIdempotencyKeyAsync(book, record, cancellationToken))
            {
                _logger.LogInformation("Created book {BookId} {Title} by author {AuthorId} with idempotency key {IdempotencyKey}",
                    book.Id, book.Title, book.AuthorId, idempotencyKey);
                return new CreateBookResult(CreateBookOutcome.Created, BookMapper.ToBookResponse(book));
            }

            // The key was taken, so nothing was saved. Either this is a retry of that earlier
            // request, or the key was reused for something else - the stored hash tells which.
            var existing = (await _bookRepository.GetIdempotencyRecordAsync(idempotencyKey, cancellationToken))!;
            if (existing.RequestHash != requestHash)
            {
                _logger.LogInformation("Idempotency key {IdempotencyKey} was reused for a different request", idempotencyKey);
                return new CreateBookResult(CreateBookOutcome.KeyReusedWithDifferentRequest, null);
            }

            var existingBook = await _bookRepository.GetByIdAsync(existing.BookId, cancellationToken);
            _logger.LogInformation("Replayed book {BookId} for idempotency key {IdempotencyKey}",
                existing.BookId, idempotencyKey);
            return new CreateBookResult(CreateBookOutcome.Replayed, BookMapper.ToBookResponse(existingBook!));
        }

        // Hashes the request as the API understood it, not the raw bytes: the same book sent with
        // different spacing or field order is still the same request.
        private static string HashRequest(CreateBookRequest createBookRequest)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(createBookRequest);
            return Convert.ToHexString(SHA256.HashData(json));
        }

        public async Task<bool> DeleteBookAsync(Guid id, CancellationToken cancellationToken)
        {

            var success = await _bookRepository.DeleteByIdAsync(id, cancellationToken);

            // The only Warning in the project. Nothing failed - but this is the one
            // irreversible operation, so it is the line I would want to find without
            // knowing in advance that I was looking for it.
            if (success) _logger.LogWarning("Deleted book {BookId}", id);
            else
            {
                _logger.LogInformation("Delete requested for unknown book {BookId}", id);
            }
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
            // Deliberately not logged. A miss on a read is the most common thing that
            // happens to a public API - stale links, typos, bots - and a line per miss
            // would be the noisiest log here while telling me nothing I would act on.
            var book =  await _bookRepository.GetByIdAsync(id, cancellationToken);
            if (book == null) {
                return null;
            }
            return BookMapper.ToBookResponse(book);
        }

        public async Task<BookResponse?> UpdateBookAsync(UpdateBookRequest updateBookrequest, Guid id, CancellationToken cancellationToken)
        {
            
            var book = await _bookRepository.GetByIdAsync(id, cancellationToken);
            if (book == null) {
                _logger.LogInformation("Update requested for unknown book {BookId}", id);
                return null;
                }
            var previousTitle = book.Title;

            book.Update(updateBookrequest.Title, updateBookrequest.AuthorId, updateBookrequest.PublicationYear, updateBookrequest.Genre);
            await _bookRepository.UpdateAsync(book, cancellationToken);
            _logger.LogInformation("Updated book {BookId} from {PreviousTitle} to {Title}",
                id, previousTitle, book.Title);

            return BookMapper.ToBookResponse(book);
        }
    }
}
