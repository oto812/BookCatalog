using BookCatalog.Application.DTOs.Requests;
using BookCatalog.Application.Interfaces;
using BookCatalog.Application.Results;
using BookCatalog.Application.Services;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;


namespace BookCatalog.Tests
{
    public class LoanServiceTests
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IBookRepository _bookRepository;
        private readonly LoanService _sut; 
        
        public LoanServiceTests() {
            _loanRepository = Substitute.For<ILoanRepository>();
            _bookRepository = Substitute.For<IBookRepository>();
            _sut = new LoanService(_loanRepository, _bookRepository, Substitute.For<ILogger<LoanService>>());
        }

        [Fact]
        public async Task BorrowBookAsync_ReturnsBookNotFound_WhenBookDoesnotExist()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var BookId = Guid.NewGuid();
            var borrowBookRequest = new BorrowBookRequest(userId, BookId);

            _bookRepository.GetByIdAsync(BookId).Returns((Book?)null);
            //Act
            var borrowBookResult = await _sut.BorrowBookAsync(borrowBookRequest);
            
            //Assert
            Assert.Equal(BorrowOutcome.BookNotFound, borrowBookResult.Outcome);
            Assert.Null(borrowBookResult.Loan);
        }

        [Fact]
        public async Task BorrowBooksAsync_ReturnsBookAlreadyBorrowed_WhenBookIsNotReturned() {
            //Arrange
            var book = new Book("title", Guid.NewGuid(), 2000, Genre.Fantasy);
            var userId = Guid.NewGuid();
            var bookId = book.Id;
            var borrowBookRequest = new BorrowBookRequest(userId, bookId);
            
            

            _bookRepository.GetByIdAsync(bookId).Returns(book);

            _loanRepository.AddAsync(Arg.Any<Loan>()).Returns(false);

            //Act
            var borrowBookResult = await _sut.BorrowBookAsync(borrowBookRequest);

            //Assert
            Assert.Equal(BorrowOutcome.BookAlreadyBorrowed, borrowBookResult.Outcome);
            Assert.Null(borrowBookResult.Loan);
        }

        [Fact]
        public async Task BorrowBooksAsync_ReturnsLoanResponse_WhenBorrowIsSuccessful()
        {
            //Arrange
            var book = new Book("title", Guid.NewGuid(), 2000, Genre.Fantasy);
            var userId = Guid.NewGuid();
            var bookId = book.Id;
            var borrowBookRequest = new BorrowBookRequest(userId, bookId);
            Loan? createdLoan = null;

            _bookRepository.GetByIdAsync(bookId).Returns(book);

            _loanRepository.AddAsync(Arg.Do<Loan>(loan => createdLoan = loan)).Returns(true);


            //Act
            var borrowBookResult = await _sut.BorrowBookAsync(borrowBookRequest);

            //Assert
            Assert.Equal(BorrowOutcome.Success,borrowBookResult.Outcome);
            Assert.Equal(book.Title, borrowBookResult.Loan!.BookTitle);
            Assert.Equal(createdLoan!.BorrowedAt, borrowBookResult.Loan.BorrowedAt);
            Assert.Equal(createdLoan.ReturnedAt, borrowBookResult.Loan.ReturnedAt);

        }

        [Fact]
        public async Task ReturnBookAsync_ReturnsLoanNotFound_WhenLoanDoesnotExists()
        {
            //Arrange
            var loanId = Guid.NewGuid();

            _loanRepository.GetByIdAsync(loanId).Returns((Loan?) null);

            //Act
            var returnBookResult = await _sut.ReturnBookAsync(loanId);

            //Assert
            Assert.Equal(ReturnBookOutcome.LoanNotFound, returnBookResult.Outcome);
            Assert.Null(returnBookResult.Loan);
        }


        [Fact]
        public async Task ReturnBookAsync_ReturnsAlreadyReturned_WhenReturnedAtIsNotNull()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var loan = new Loan(userId,bookId);
            loan.Return();
            

            _loanRepository.GetByIdAsync(loan.Id).Returns(loan);


            //Act
            var returnBookResult = await _sut.ReturnBookAsync(loan.Id);


            //Assert
            Assert.Equal(ReturnBookOutcome.AlreadyReturned, returnBookResult.Outcome);
            Assert.Null(returnBookResult.Loan);
        }
    }
}
