using BookCatalog.Application.DTOs.Queries;
using BookCatalog.Application.Interfaces;
using BookCatalog.Domain.Entities;
using BookCatalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace BookCatalog.Infrastructure.Repositories
{
    public class EfBookRepository : IBookRepository
    {
        private readonly BookCatalogDbContext _dbContext;

        public EfBookRepository(BookCatalogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Book> AddAsync(Book book)
        {
            var dbBook = _dbContext.Books.Add(book);
            await _dbContext.SaveChangesAsync();
            return dbBook.Entity;
        }

        public async Task<bool> DeleteByIdAsync(Guid id)
        {
            var book = await _dbContext.Books.FindAsync(id);

            if (book == null) {
                return false;
            }

            _dbContext.Remove(book);
            await _dbContext.SaveChangesAsync();

            return true;


        }

        public async Task<(IEnumerable<Book> Books, int TotalBooks)> GetAllAsync(GetBooksQuery getBooksQuery)
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
            var total = await query.CountAsync();
            var books = await query.OrderBy(b => b.CreatedAt)
                .Skip((getBooksQuery.Page - 1) * getBooksQuery.PageSize)
                .Take(getBooksQuery.PageSize)
                .ToListAsync();
            
            var result = (Books: books, TotalBooks: total);
            return result;
        }

        public async Task<Book?> GetByIdAsync(Guid id)
        {
            var book = await _dbContext.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
            return book;
        }

        public async Task UpdateAsync(Book book)
        {
            _dbContext.Update(book);

            await _dbContext.SaveChangesAsync();

        }
    }
}
