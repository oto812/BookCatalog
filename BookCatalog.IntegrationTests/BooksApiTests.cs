using BookCatalog.Application.DTOs.Requests;
using BookCatalog.Application.DTOs.Responses;
using BookCatalog.Domain.Enums;
using BookCatalog.Infrastructure.Persistence;
using BookCatalog.IntegrationTests.TestHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;


namespace BookCatalog.IntegrationTests;


public class BooksApiTests : IClassFixture<BookCatalogApiFactory>, IAsyncLifetime
{
    private readonly BookCatalogApiFactory _factory;
    private readonly HttpClient _client;

    public BooksApiTests(BookCatalogApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateBook_ThenFetchById_ReturnsTheCreatedBook()
    {
        // ARRANGE
        var authorId = await AuthorHelper.GiveAnAuthorAsync(_factory);
        var request = new CreateBookRequest("Dune", authorId, 1965, Genre.Science);

        // ACT
        var postResponse = await _client.PostAsJsonAsync("/api/books", request);
        var created = await postResponse.Content.ReadFromJsonAsync<BookResponse>();

        var getResponse = await _client.GetAsync($"/api/books/{created!.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<BookResponse>();

        // ASSERT
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal("Dune", fetched.Title);
        Assert.Equal(authorId, fetched.AuthorId);
        Assert.Equal(1965, fetched.PublicationYear);
        Assert.Equal(Genre.Science, fetched.Genre);
    }
    [Fact]
    public async Task GetBookById_Returns404_WhenBookDoesNotExist()
    {
        //ARRANGE
        var randomId = Guid.NewGuid();
        //ACT
        var getResponse = await _client.GetAsync($"/api/books/{randomId}");
        //ASSERT
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
    [Fact]
    public async Task CreateBook_Returns400_WhenTitleMissing()
    {
        //ARRANGE
        var request = new CreateBookRequest("", Guid.NewGuid(), 2020, Genre.Fantasy);
        //ACT
        var postResponse = await _client.PostAsJsonAsync("/api/books", request);
        //ASSERT
        Assert.Equal(HttpStatusCode.BadRequest, postResponse.StatusCode);
    }
    [Fact]
    public async Task CreateBook_Returns400_WhenPublicationYearIsInFuture()
    {
        //ARRANGE
        var publicationYearInFuture = DateTime.Now.Year + 1;
        var request = new CreateBookRequest("Future Book", Guid.NewGuid(), publicationYearInFuture, Genre.Fantasy);
        //ACT
        var postResponse = await _client.PostAsJsonAsync("/api/books", request);
        //ASSERT
        Assert.Equal(HttpStatusCode.BadRequest, postResponse.StatusCode);
    }

    [Fact]
    public async Task GetAllBooks_ReturnsOnePage_andTheFullTotal()
    {
        //ARRANGE
        var authorId = await AuthorHelper.GiveAnAuthorAsync(_factory);
        var request1 = new CreateBookRequest("Dune", authorId, 1965, Genre.Science);
        var request2 = new CreateBookRequest("Dune Messiah", authorId, 1969, Genre.Science);
        var request3 = new CreateBookRequest("Children of Dune", authorId, 1976, Genre.Science);
        var request4 = new CreateBookRequest("God Emperor of Dune", authorId, 1981, Genre.Science);

        await _client.PostAsJsonAsync("/api/books", request1);
        await _client.PostAsJsonAsync("/api/books", request2);
        await _client.PostAsJsonAsync("/api/books", request3);
        await _client.PostAsJsonAsync("/api/books", request4);

        // ACT
        var getResponse = await _client.GetAsync("/api/books?page=2&pageSize=2");
        var pagedResponse = await getResponse.Content.ReadFromJsonAsync<PagedBooksResponse>();
        // ASSERT
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(pagedResponse);
        Assert.Equal(2,pagedResponse!.Books.Count()); 
        Assert.Equal(4, pagedResponse.TotalBooks); 
    }

    [Fact]
    public async Task GetAllBooks_FiltersByAuthor()
    {
        //ARRANGE
        var authorId1 = await AuthorHelper.GiveAnAuthorAsync(_factory);
        var authorId2 = await AuthorHelper.GiveAnAuthorAsync(_factory);
        var request1 = new CreateBookRequest("Dune", authorId1, 1965, Genre.Science);
        var request2 = new CreateBookRequest("Dune Messiah", authorId1, 1969, Genre.Science);
        var request3 = new CreateBookRequest("Children of Dune", authorId2, 1976, Genre.Science);
        await _client.PostAsJsonAsync("/api/books", request1);
        await _client.PostAsJsonAsync("/api/books", request2);
        await _client.PostAsJsonAsync("/api/books", request3);
        //ACT
        var getResponse = await _client.GetAsync($"/api/books?authorId={authorId1}");
        var pagedResponse = await getResponse.Content.ReadFromJsonAsync<PagedBooksResponse>();
        //ASSERT
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(pagedResponse);
        Assert.Equal(2, pagedResponse!.Books.Count());
        Assert.All(pagedResponse.Books, book => Assert.Equal(authorId1, book.AuthorId));
    }
    [Fact]
    public async Task UpdateBook_ChangesTheStoredBook()
    {
        //ARRANGE
        var authorId = await AuthorHelper.GiveAnAuthorAsync(_factory);
        var updatedAuthorId = await AuthorHelper.GiveAnAuthorAsync(_factory);
        var request = new CreateBookRequest("Dune", authorId, 1965, Genre.Science);
        var updateBookRequest = new UpdateBookRequest("Dune Updated", updatedAuthorId, 1966, Genre.Fantasy);
        
        var postBookResponse = await _client.PostAsJsonAsync("/api/books", request);
        var createdBook = await postBookResponse.Content.ReadFromJsonAsync<BookResponse>();
        //ACT
        var updateResponse = await _client.PutAsJsonAsync($"/api/books/{createdBook!.Id}", updateBookRequest);
        var updated = await updateResponse.Content.ReadFromJsonAsync<BookResponse>();

        var fetch = await _client.GetAsync($"/api/books/{updated!.Id}");

        var getResponse = await fetch.Content.ReadFromJsonAsync<BookResponse>();


        //ASSERT
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, fetch.StatusCode);

        Assert.Equal(updated!.Id, getResponse!.Id);
        Assert.Equal(updateBookRequest.Title, getResponse.Title);
        Assert.Equal(updateBookRequest.AuthorId, getResponse.AuthorId);
        Assert.Equal(updateBookRequest.PublicationYear, getResponse.PublicationYear);
        Assert.Equal(updateBookRequest.Genre, getResponse.Genre);
    }

    [Fact]
    public async Task DeleteBook_RemovesTheBook()
    {
        //ARRANGE
        var authorId = await AuthorHelper.GiveAnAuthorAsync(_factory);
        var request = new CreateBookRequest("Dune", authorId, 1965, Genre.Science);
        var postResponse = await _client.PostAsJsonAsync("/api/books", request);
        var created = await postResponse.Content.ReadFromJsonAsync<BookResponse>();
        //ACT
        var deleteResponse = await _client.DeleteAsync($"/api/books/{created!.Id}");
        var fetchResponse = await _client.GetAsync($"/api/books/{created.Id}");
        //ASSERT
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, fetchResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_Returns404_WhenBookDoesNotExist()
    {
        //ARRANGE
        var randomId = Guid.NewGuid();
        //ACT
        var deleteResponse = await _client.DeleteAsync($"/api/books/{randomId}");
        //ASSERT
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    // A retry after a lost response: same key, same body. The book must exist once, and the
    // retry must get the original book back, not a new one.
    [Fact]
    public async Task CreateBook_TwiceWithSameIdempotencyKey_CreatesOneBook()
    {
        //ARRANGE
        var authorId = await AuthorHelper.GiveAnAuthorAsync(_factory);
        var request = new CreateBookRequest("Dune", authorId, 1965, Genre.Science);
        var idempotencyKey = Guid.NewGuid().ToString();

        //ACT
        var firstResponse = await PostBookAsync(request, idempotencyKey);
        var secondResponse = await PostBookAsync(request, idempotencyKey);
        var first = await firstResponse.Content.ReadFromJsonAsync<BookResponse>();
        var second = await secondResponse.Content.ReadFromJsonAsync<BookResponse>();

        //ASSERT
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        Assert.Equal(first!.Id, second!.Id);
        Assert.Equal(firstResponse.Headers.Location, secondResponse.Headers.Location);

        // Idempotency is about state, not responses: check what is actually stored.
        Assert.Equal(1, await CountBooksAsync("Dune"));
    }

    // Two different operations that happen to send the same fields. Different keys mean the
    // client intends two books, so both are created - the key decides, not the content.
    [Fact]
    public async Task CreateBook_WithDifferentIdempotencyKeys_CreatesTwoBooks()
    {
        //ARRANGE
        var authorId = await AuthorHelper.GiveAnAuthorAsync(_factory);
        var request = new CreateBookRequest("Dune", authorId, 1965, Genre.Science);

        //ACT
        var firstResponse = await PostBookAsync(request, Guid.NewGuid().ToString());
        var secondResponse = await PostBookAsync(request, Guid.NewGuid().ToString());
        var first = await firstResponse.Content.ReadFromJsonAsync<BookResponse>();
        var second = await secondResponse.Content.ReadFromJsonAsync<BookResponse>();

        //ASSERT
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        Assert.NotEqual(first!.Id, second!.Id);
        Assert.Equal(2, await CountBooksAsync("Dune"));
    }

    // A key reused for a different request is a client bug. Replaying the first book would hide
    // it, and creating the second would break the promise the key makes, so it is rejected.
    [Fact]
    public async Task CreateBook_SameIdempotencyKeyWithDifferentBody_Returns422()
    {
        //ARRANGE
        var authorId = await AuthorHelper.GiveAnAuthorAsync(_factory);
        var idempotencyKey = Guid.NewGuid().ToString();
        var request = new CreateBookRequest("Dune", authorId, 1965, Genre.Science);
        var differentRequest = new CreateBookRequest("Dune Messiah", authorId, 1969, Genre.Science);

        //ACT
        var firstResponse = await PostBookAsync(request, idempotencyKey);
        var secondResponse = await PostBookAsync(differentRequest, idempotencyKey);

        //ASSERT
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, secondResponse.StatusCode);
        Assert.Equal(1, await CountBooksAsync("Dune"));
        Assert.Equal(0, await CountBooksAsync("Dune Messiah"));
    }

    private Task<HttpResponseMessage> PostBookAsync(CreateBookRequest request, string idempotencyKey)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, "/api/books")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("Idempotency-Key", idempotencyKey);

        return _client.SendAsync(message);
    }

    private async Task<int> CountBooksAsync(string title)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookCatalogDbContext>();

        return await db.Books.CountAsync(b => b.Title == title);
    }
}
