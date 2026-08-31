using BookCatalog.Domain.Entities;
using BookCatalog.Application.DTOs.Requests;
using BookCatalog.Application.DTOs.Responses;
using BookCatalog.Application.Interfaces;
using Microsoft.Extensions.Logging;
using BookCatalog.Application.Results;


namespace BookCatalog.Application.Services
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<LoanService> _logger;
        
        public LoanService(ILoanRepository loanRepository, IBookRepository bookRepository, ILogger<LoanService> logger)
        {
            _loanRepository = loanRepository;
            _bookRepository = bookRepository;
            _logger = logger;
        }
        public async Task<BorrowBookResult> BorrowBookAsync(BorrowBookRequest borrowBookRequest)
        {
            //I should check if the user exists, but for now I will just check if the book exists
            var book = await _bookRepository.GetByIdAsync(borrowBookRequest.BookId);
            if(book == null)
            {
                return new BorrowBookResult(BorrowOutcome.BookNotFound, null);
            }
            var loan = new Loan(borrowBookRequest.UserId, borrowBookRequest.BookId);
            var success = await _loanRepository.AddAsync(loan);
            if (!success)
            {
                _logger.LogInformation("Book {BookId} is already borrowed", borrowBookRequest.BookId);
                return new BorrowBookResult(BorrowOutcome.BookAlreadyBorrowed, null); 
            }
            _logger.LogInformation("User {UserId} borrowed book {BookId} at {BorrowedAt}", borrowBookRequest.UserId, borrowBookRequest.BookId, loan.BorrowedAt);
            var loanResponse = new LoanResponse(loan.Id, book.Title, loan.BorrowedAt, loan.ReturnedAt);
            return new BorrowBookResult(BorrowOutcome.Success, loanResponse);
        }

        public async Task<IEnumerable<LoanResponse>> LoanHistoryAsync(Guid userId)
        {
            //I should check if the user exists, but for now I will just return the loan history
            var loans = await _loanRepository.GetAllPerUserAsync(userId);
            var loanResponse = loans.Select(loan => new LoanResponse
            (
                loan.Id,
                loan.Book.Title,
                loan.BorrowedAt,
                loan.ReturnedAt
            )).ToList();
            return loanResponse;
        }

        public async Task<ReturnBookResult> ReturnBookAsync(Guid loanId)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if(loan == null)
            {
                _logger.LogInformation("Loan {LoanId} not found", loanId);
                return new ReturnBookResult(ReturnBookOutcome.LoanNotFound, null);
            }
            else if(loan.ReturnedAt != null)
            {
                _logger.LogInformation("Loan {LoanId} has already been returned", loanId);
                return new (ReturnBookOutcome.AlreadyReturned, null);
            }
            loan.Return();
            await _loanRepository.ReturnAsync(loan);
            var loanResponse = new LoanResponse(loan.Id, loan.Book.Title, loan.BorrowedAt, loan.ReturnedAt);
            return new ReturnBookResult (ReturnBookOutcome.Success, loanResponse);
        }
    }
}
