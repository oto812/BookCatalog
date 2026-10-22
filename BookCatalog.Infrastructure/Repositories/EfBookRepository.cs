using BookCatalog.Application.DTOs.Queries;
using BookCatalog.Application.Idempotency;
using BookCatalog.Application.Interfaces;
using BookCatalog.Domain.Entities;
using BookCatalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;


namespace BookCatalog.Infrastructure.Repositories
{
    public class EfBookRepository : IBookRepository
    {
        private readonly BookCatalogDbContext _dbContext;

        public EfBookRepository(BookCatalogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Book> AddAsync(Book book, CancellationToken cancellationToken)
        {
            var dbBook = _dbContext.Books.Add(book);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return dbBook.Entity;
        }

        public async Task<bool> AddWithIdempotencyKeyAsync(Book book, IdempotencyRecord record, CancellationToken cancellationToken)
        {
            // One SaveChanges is one transaction: the book and its key are stored together or not
            // at all. If a request with the same key is still running, Postgres makes this insert
            // wait for it, then fails it on the primary key.
            _dbContext.Books.Add(book);
            _dbContext.IdempotencyRecords.Add(record);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException postgresException &&
                postgresException.ConstraintName == BookCatalogDbContext.IdempotencyKeyPrimaryKey)
            {
                // The book and record that failed are still tracked as "Added". Forget them, or a
                // lookup of this key would return this unsaved record instead of the stored one.
                _dbContext.ChangeTracker.Clear();
                return false;
            }

            return true;
        }

        public async Task<IdempotencyRecord?> GetIdempotencyRecordAsync(string key, CancellationToken cancellationToken)
        {
            return await _dbContext.IdempotencyRecords.AsNoTracking().FirstOrDefaultAsync(r => r.Key == key, cancellationToken);
        }

        public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var book = await _dbContext.Books.FindAsync([id], cancellationToken);

            if (book == null) {
                return false;
            }

            _dbContext.Remove(book);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;


        }

        public async Task<(IEnumerable<Book> Books, int TotalBooks)> GetAllAsync(GetBooksQuery getBooksQuery, CancellationToken cancellationToken)
        {
            var query = _dbContext.Books.AsNoTracking().AsQueryable();
            if (getBooksQuery.AuthorId != null)
            {
                query = query.Where(b => b.AuthorId == getBooksQuery.AuthorId);
            }
            if (getBooksQuery.Genre != null)
            {
                query = query.Where(b => b.Genre == getBooksQuery.Genre);

            }
            if (getBooksQuery.PublicationYear != null)
            {
                query = query.Where(b => b.PublicationYear == getBooksQuery.PublicationYear);
            }
            var total = await query.CountAsync(cancellationToken);
            var books = await query.OrderBy(b => b.CreatedAt)
                .Skip((getBooksQuery.Page - 1) * getBooksQuery.PageSize)
                .Take(getBooksQuery.PageSize)
                .ToListAsync(cancellationToken);
            
            var result = (Books: books, TotalBooks: total);
            return result;
        }

        public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var book = await _dbContext.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
            return book;
        }

        public async Task UpdateAsync(Book book, CancellationToken cancellationToken)
        {
            _dbContext.Update(book);

            await _dbContext.SaveChangesAsync(cancellationToken);

        }
    }
}
