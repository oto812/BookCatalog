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
        public async Task<ActionResult<LoanResponse>> BorrowBook([FromBody] BorrowBookRequest borrowBookRequest)
        {
            var response = await _loanService.BorrowBookAsync(borrowBookRequest);
            return response.Outcome switch
            {
                BorrowOutcome.BookNotFound => NotFound("..."),
                BorrowOutcome.BookAlreadyBorrowed => Conflict("..."),
                BorrowOutcome.Success => Ok(response.Loan),
                _ => throw new UnreachableException()
            };
        }

        [HttpPost("{loanId}/return")]
        public async Task<ActionResult<LoanResponse>> ReturnBook(Guid loanId)
        {
            var response = await _loanService.ReturnBookAsync(loanId);

            if(response.Outcome == ReturnBookOutcome.LoanNotFound)
            {
                return NotFound("The Loan was not found");
            }
            else if (response.Outcome == ReturnBookOutcome.AlreadyReturned)
            {
                return Conflict("The book has already been returned");
            }
            return Ok(response.Loan);
        }

        [HttpGet("~/api/users/{userId}/loans")]
        public async Task<ActionResult<IEnumerable<LoanResponse>>> LoanHistory(Guid userId)
        {
            var response = await _loanService.LoanHistoryAsync(userId);

            return Ok(response);
            
        }


    }
}
