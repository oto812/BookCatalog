using BookCatalog.Application.DTOs.Requests;
using BookCatalog.Application.DTOs.Responses;
using BookCatalog.Domain.Enums;
using BookCatalog.IntegrationTests.TestHelper;
using System.Net;
using System.Net.Http.Json;

namespace BookCatalog.IntegrationTests
{
    public class LoansApiTests : IClassFixture<BookCatalogApiFactory>, IAsyncLifetime
    {
        private readonly BookCatalogApiFactory _factory;
        private readonly HttpClient _client;
        public LoansApiTests(BookCatalogApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

        }
        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            await _factory.ResetDatabaseAsync();
        }

        [Fact]
        public async Task BorrowBook_ReturnsTheLoan_WhenBookIsAvailable()
        {
            //ARRANGE
            var authorId = await AuthorHelper.GiveAnAuthorAsync(_factory);
            var userId = await UserHelper.GiveAUserAsync(_factory);

            var createBookRequest = new CreateBookRequest("Dune", authorId, 1965, Genre.Science);
            var postBookResponse = await _client.PostAsJsonAsync("/api/books", createBookRequest);
            var createdBook = await postBookResponse.Content.ReadFromJsonAsync<BookResponse>();
            
            var borrowBookRequest = new BorrowBookRequest(userId, createdBook!.Id);


            //ACT
            var borrowResponse = await _client.PostAsJsonAsync("/api/loans", borrowBookRequest);

            var createdLoan = await borrowResponse.Content.ReadFromJsonAsync<LoanResponse>();
            //ASSERT

            Assert.Equal(HttpStatusCode.OK, borrowResponse.StatusCode);
            Assert.Equal(createdLoan!.BookTitle, createdBook.Title);
            Assert.Null(createdLoan!.ReturnedAt);

        }

        [Fact]
        public async Task BorrowBook_Returns409_WhenBookIsAlreadyBorrowed()
        {
            //ARRANGE
            var authorId = await AuthorHelper.GiveAnAuthorAsync(_factory);
            var userId1 = await UserHelper.GiveAUserAsync(_factory);
            var userId2 = await UserHelper.GiveAUserAsync(_factory);
            var createBookRequest = new CreateBookRequest("Dune", authorId, 1965, Genre.Science);
            var postBookResponse = await _client.PostAsJsonAsync("/api/books", createBookRequest);
            var createdBook = await postBookResponse.Content.ReadFromJsonAsync<BookResponse>();
            var borrowBookRequest1 = new BorrowBookRequest(userId1, createdBook!.Id);
            var borrowBookRequest2 = new BorrowBookRequest(userId2, createdBook!.Id);
            //ACT
            var borrowResponse1 = await _client.PostAsJsonAsync("/api/loans", borrowBookRequest1);
            var borrowResponse2 = await _client.PostAsJsonAsync("/api/loans", borrowBookRequest2);
            //ASSERT
            Assert.Equal(HttpStatusCode.OK, borrowResponse1.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, borrowResponse2.StatusCode);
        }

        [Fact]
        public async Task BorrowBook_Returns404_WhenBookDoesNotExist()
        {
            //ARRANGE
            var userId = await UserHelper.GiveAUserAsync(_factory);
            var randomBookId = Guid.NewGuid();
            var borrowBookRequest = new BorrowBookRequest(userId, randomBookId);
            //ACT
            var borrowResponse = await _client.PostAsJsonAsync("/api/loans", borrowBookRequest);
            //ASSERT
            Assert.Equal(HttpStatusCode.NotFound, borrowResponse.StatusCode);
        }
        [Fact]
        public async Task ReturnBook_SetsReturnedAt()
        {
            var userId = await UserHelper.GiveAUserAsync(_factory);
            var authorId = await AuthorHelper.GiveAnAuthorAsync(_factory);

            var createBookRequest = new CreateBookRequest("Dune", authorId, 1965, Genre.Science);

            var postBookResponse = await _client.PostAsJsonAsync("/api/books", createBookRequest);
            var createdBook = await postBookResponse.Content.ReadFromJsonAsync<BookResponse>();
            var borrowBookRequest = new BorrowBookRequest(userId, createdBook!.Id);

            var borrowResponse = await _client.PostAsJsonAsync("/api/loans", borrowBookRequest);

            var createdLoan = await borrowResponse.Content.ReadFromJsonAsync<LoanResponse>();

            //ACT
            var returnBookResponse = await _client.PostAsync($"/api/loans/{createdLoan!.LoanId}/return", null);

            var updatedLoan = await returnBookResponse.Content.ReadFromJsonAsync<LoanResponse>();

            //ASSERT
            Assert.NotNull(updatedLoan!.ReturnedAt);
        }

        [Fact]
        public async Task ReturnBook_Returns409_WhenAlreadyReturned()
        {
            var userId = await UserHelper.GiveAUserAsync(_factory);
            var authorId = await AuthorHelper.GiveAnAuthorAsync(_factory);

            var createBookRequest = new CreateBookRequest("Dune", authorId, 1965, Genre.Science);
            var postBookResponse = await _client.PostAsJsonAsync("/api/books", createBookRequest);
            var createdBook = await postBookResponse.Content.ReadFromJsonAsync<BookResponse>();
            var borrowBookRequest = new BorrowBookRequest(userId, createdBook!.Id);
            var borrowResponse = await _client.PostAsJsonAsync("/api/loans", borrowBookRequest);
            var createdLoan = await borrowResponse.Content.ReadFromJsonAsync<LoanResponse>();
            //ACT
            var returnBookResponse1 = await _client.PostAsync($"/api/loans/{createdLoan!.LoanId}/return", null);
            var returnBookResponse2 = await _client.PostAsync($"/api/loans/{createdLoan!.LoanId}/return", null);
            //ASSERT
            Assert.Equal(HttpStatusCode.OK, returnBookResponse1.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, returnBookResponse2.StatusCode);
        }

        [Fact]
        public async Task LoanHistory_ReturnsTheUsersLoans()
        {
            var authorId = await AuthorHelper.GiveAnAuthorAsync(_factory);

            var userId = await UserHelper.GiveAUserAsync(_factory);
            var createBookRequest1 = new CreateBookRequest("Dune", authorId, 1965, Genre.Science);
            var createBookRequest2 = new CreateBookRequest("1984", authorId, 1949, Genre.Fantasy);
            var postBookResponse1 = await _client.PostAsJsonAsync("/api/books", createBookRequest1);
            var postBookResponse2 = await _client.PostAsJsonAsync("/api/books", createBookRequest2);
            var createdBook1 = await postBookResponse1.Content.ReadFromJsonAsync<BookResponse>();
            var createdBook2 = await postBookResponse2.Content.ReadFromJsonAsync<BookResponse>();
            var borrowBookRequest1 = new BorrowBookRequest(userId, createdBook1!.Id);
            var borrowBookRequest2 = new BorrowBookRequest(userId, createdBook2!.Id);
            await _client.PostAsJsonAsync("/api/loans", borrowBookRequest1);
            await _client.PostAsJsonAsync("/api/loans", borrowBookRequest2);
            //ACT
            var loanHistoryResponse = await _client.GetAsync($"/api/users/{userId}/loans");
            var loanHistory = await loanHistoryResponse.Content.ReadFromJsonAsync<IEnumerable<LoanResponse>>();
            //ASSERT
            Assert.Equal(HttpStatusCode.OK, loanHistoryResponse.StatusCode);
            Assert.NotNull(loanHistory);
            Assert.Equal(2, loanHistory!.Count());
        }

        [Fact]
        public async Task BorrowBook_Succeeds_AfterTheBookIsReturned()
        {
            //ARRANGE
            var authorId = await AuthorHelper.GiveAnAuthorAsync(_factory);
            var userId1 = await UserHelper.GiveAUserAsync(_factory);
            var userId2 = await UserHelper.GiveAUserAsync(_factory);

            var createBookRequest = new CreateBookRequest("Dune", authorId, 1965, Genre.Science);
            var postBookResponse = await _client.PostAsJsonAsync("/api/books", createBookRequest);
            var createdBook = await postBookResponse.Content.ReadFromJsonAsync<BookResponse>();

            var borrowBookRequest1 = new BorrowBookRequest(userId1, createdBook!.Id);
            var borrowBookRequest2 = new BorrowBookRequest(userId2, createdBook!.Id);
            //ACT
            var borrowResponse1 = await _client.PostAsJsonAsync("/api/loans", borrowBookRequest1);
            var createdLoan1 = await borrowResponse1.Content.ReadFromJsonAsync<LoanResponse>();
            var returnResponse = await _client.PostAsync($"/api/loans/{createdLoan1!.LoanId}/return", null);
            var borrowResponse2 = await _client.PostAsJsonAsync("/api/loans", borrowBookRequest2);

            //ASSERT
            Assert.Equal(HttpStatusCode.OK, borrowResponse1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, returnResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, borrowResponse2.StatusCode);
        }
    }
}
