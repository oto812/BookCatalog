using BookCatalog.Application.DTOs.Requests;
using BookCatalog.Application.DTOs.Responses;
using BookCatalog.Application.Results;
using BookCatalog.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookCatalog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase 
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpPost]
        public async Task<ActionResult<LoanResponse>> BorrowBook([FromBody] BorrowBookRequest borrowBookRequest, CancellationToken cancellationToken)
        {
            var response = await _loanService.BorrowBookAsync(borrowBookRequest, cancellationToken);
            return response.Outcome switch
            {
                BorrowOutcome.BookNotFound => NotFound("The book was not found"),
                BorrowOutcome.BookAlreadyBorrowed => Conflict("The book is already borrowed"),
                BorrowOutcome.Success => Ok(response.Loan),
                _ => throw new UnreachableException()
            };
        }

        [HttpPost("{loanId}/return")]
        public async Task<ActionResult<LoanResponse>> ReturnBook(Guid loanId, CancellationToken cancellationToken)
        {
            var response = await _loanService.ReturnBookAsync(loanId, cancellationToken);
            return response.Outcome switch
            {
                ReturnBookOutcome.LoanNotFound => NotFound("The Loan was not found"),
                ReturnBookOutcome.AlreadyReturned => Conflict("The book has already been returned"),
                ReturnBookOutcome.Success => Ok(response.Loan),
                _ => throw new UnreachableException()
            };
        }

        [HttpGet("~/api/users/{userId}/loans")]
        public async Task<ActionResult<IEnumerable<LoanResponse>>> LoanHistory(Guid userId, CancellationToken cancellationToken)
        {
            var response = await _loanService.LoanHistoryAsync(userId, cancellationToken);

            return Ok(response);
            
        }
    
    
    }
}
