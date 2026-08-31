using BookCatalog.Application.Interfaces;
using BookCatalog.Domain.Entities;
using BookCatalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;


namespace BookCatalog.Infrastructure.Repositories
{
    public class EfLoanRepository : ILoanRepository
    {
        private readonly BookCatalogDbContext _dbContext;

        public EfLoanRepository(BookCatalogDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<bool> AddAsync(Loan loan)
        {
            var Dbloan = _dbContext.Add(loan);
            try
            {
                await _dbContext.SaveChangesAsync();
            }catch(DbUpdateException ex)
               when (ex.InnerException is PostgresException postgresException && 
               postgresException.ConstraintName == BookCatalogDbContext.ActiveLoanIndex)
            {
                
                return false;
            }
            
            return true;
        }

        public async Task<IEnumerable<Loan>> GetAllPerUserAsync(Guid userId)
        {
            var loans = await  _dbContext.Loans
                .Where(l => l.UserId == userId)
                .Include(l => l.Book)
                .OrderBy(l => l.BorrowedAt)
                .ToListAsync();

            return loans;
        }

        public async Task<Loan?> GetByIdAsync(Guid id)
        {
            var loan = await _dbContext.Loans.AsNoTracking().Include(l => l.Book).FirstOrDefaultAsync(l => l.Id == id);
            return loan;
        }

        public async Task ReturnAsync(Loan loan)
        {
            _dbContext.Loans.Update(loan);

            await _dbContext.SaveChangesAsync();

            
        }
    }
}
