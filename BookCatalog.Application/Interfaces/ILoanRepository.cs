
using BookCatalog.Domain.Entities;

namespace BookCatalog.Application.Interfaces
{
    public interface ILoanRepository
    {
        public Task<bool> AddAsync(Loan loan, CancellationToken cancellationToken);
        public Task ReturnAsync(Loan loan, CancellationToken cancellationToken);
        public Task<IEnumerable<Loan>> GetAllPerUserAsync(Guid userId, CancellationToken cancellationToken);
        public Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
