
using BookCatalog.Domain.Entities;

namespace BookCatalog.Application.Interfaces
{
    public interface ILoanRepository
    {
        public Task<bool> AddAsync(Loan loan);
        public Task ReturnAsync(Loan loan);
        public Task<IEnumerable<Loan>> GetAllPerUserAsync(Guid userId);
        public Task<Loan?> GetByIdAsync(Guid id);
    }
}
