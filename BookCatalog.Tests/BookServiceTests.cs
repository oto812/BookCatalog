using BookCatalog.Application.DTOs.Queries;
using BookCatalog.Application.DTOs.Requests;
using BookCatalog.Application.Interfaces;
using BookCatalog.Application.Services;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BookCatalog.Tests;

public class BookServiceTests
{
    private readonly IBookRepository _repository;
    private readonly BookService _sut; // sut = "system under test", the thing being tested

    
    public BookServiceTests()
    {
        _repository = Substitute.For<IBookRepository>();
        _sut = new BookService(_repository, Substitute.For<ILogger<BookService>>());
    }

    [Fact]
    public async Task GetBookById_ReturnsNull_WhenBookDoesNotExist()
    {
        // ARRANGE 
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id).Returns((Book?)null);

        // ACT
        var result = await _sut.GetBookByIdAsync(id);

        // ASSERT 
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBookById_ReturnsMappedResponse_WhenBookExists()
    {
        // ARRANGE 
        var authorId = Guid.NewGuid();
        var book = new Book("Dune", authorId, 1965, Genre.Fantasy);
        _repository.GetByIdAsync(book.Id).Returns(book);

        // ACT
        var result = await _sut.GetBookByIdAsync(book.Id);

        // ASSERT 
        Assert.NotNull(result);
        Assert.Equal(book.Id, result.Id);
        Assert.Equal("Dune", result.Title);
        Assert.Equal(authorId, result.AuthorId);
        Assert.Equal(1965, result.PublicationYear);
        Assert.Equal(Genre.Fantasy, result.Genre);
    }

    [Fact]
    public async Task DeleteBook_ReturnsFalse_WhenBookDoesNotExist()
    {
        // ARRANGE
        var id = Guid.NewGuid();
        _repository.DeleteByIdAsync(id).Returns(false);

        // ACT
        var result = await _sut.DeleteBookAsync(id);

        // ASSERT 
        Assert.False(result);

       
    }

    [Fact]
    public async Task DeleteBook_ReturnsTrue_WhenBookExists()
    {
        // ARRANGE
        var id = Guid.NewGuid();
        _repository.DeleteByIdAsync(id).Returns(true);
        // ACT

        var result = await _sut.DeleteBookAsync(id);

        //ASSERT
        Assert.True(result);
    }

    [Fact]
    public async Task AddBook_ReturnsNull_WhenRepositoryFails()
    {
        // ARRANGE
        var request = new CreateBookRequest("Dune", Guid.NewGuid(), 1965, Genre.Fantasy);
        _repository.AddAsync(Arg.Any<Book>()).Returns((Book?)null);
        // ACT
        var result = await _sut.AddBookAsync(request);
        // ASSERT
        Assert.Null(result);
    }

    [Fact]
    public async Task AddBook_ReturnsResponse_WhenRepositorySucceeds()
    {
        // ARRANGE
        var request = new CreateBookRequest("Dune", Guid.NewGuid(), 1965, Genre.Fantasy);
        var book = new Book(request.Title, request.AuthorId, request.PublicationYear, request.Genre);
        _repository.AddAsync(Arg.Any<Book>()).Returns(callInfo => callInfo.Arg<Book>());
        // ACT
        var result = await _sut.AddBookAsync(request);
        // ASSERT
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(book.Title, result.Title);
        Assert.Equal(book.AuthorId, result.AuthorId);
        Assert.Equal(book.PublicationYear, result.PublicationYear);
        Assert.Equal(book.Genre, result.Genre);
    }

    [Fact]
    public async Task UpdateBook_ReturnsNull_WhenBookDoesNotExist()
    {
        // ARRANGE
        var id = Guid.NewGuid();
        var request = new UpdateBookRequest("Dune", Guid.NewGuid(), 1965, Genre.Fantasy);
        _repository.GetByIdAsync(id).Returns((Book?)null);

        //ACT
        var result = await _sut.UpdateBookAsync(request, id);

        //ASSERT
        Assert.Null(result);
    }

    //[Fact]
    //public async Task UpdateBook_ReturnsNull_WhenConcurrentUpdateFails()
    //{
    //    // ARRANGE
    //    var id = Guid.NewGuid();
    //    var request = new UpdateBookRequest("Dune", Guid.NewGuid(), 1965, Genre.Fantasy);
    //    var existingBook = new Book("Old Title", Guid.NewGuid(), 1900, Genre.Fantasy);
    //    _repository.GetByIdAsync(id).Returns(existingBook);

    //    _repository.UpdateAsync(Arg.Any<UpdateBookRequest>(), id).Returns((Book?)null);
    //    //Act
    //    var result = await _sut.UpdateBookAsync(request, id);

    //    //ASSERT
    //    Assert.Null(result);
    //}
    //[Fact]
    //public async Task UpdateBook_ReturnsUpdatedResponse_WhenUpdateSucceeds()
    //{
    //    // ARRANGE
        
    //    var newAuthorId = Guid.NewGuid();
    //    var bookId = Guid.NewGuid();
    //    var request = new UpdateBookRequest("New Title", newAuthorId, 2000, Genre.Science);
    //    var updatedBook = new Book("New Title", newAuthorId, 2000, Genre.Science);

    //    _repository.UpdateAsync(Arg.Any<UpdateBookRequest>(), Arg.Any<Guid>())
    //        .Returns(updatedBook);

    //    // ACT
    //    var result = await _sut.UpdateBookAsync(request, bookId);

    //    // ASSERT 
    //    Assert.NotNull(result);
    //    Assert.Equal(updatedBook.Id, result.Id);
    //    Assert.Equal("New Title", result.Title);
    //    Assert.Equal(newAuthorId, result.AuthorId);
    //    Assert.Equal(2000, result.PublicationYear);
    //    Assert.Equal(Genre.Science, result.Genre);
    //}

    [Fact]
    public async Task GetAllBooks_ReturnsPagedResponse()
    {
        //ARRANGE
        var firstBookAuthorId = Guid.NewGuid();
        var secondBookAuthorId = Guid.NewGuid();
        var books = new List<Book>
    {
        new Book("Dune",firstBookAuthorId , 1965, Genre.Fantasy),
        new Book("It", secondBookAuthorId, 1986, Genre.Horror)
    };
        var query = new GetBooksQuery(null, null, null, 1, 10);
        _repository.GetAllAsync(Arg.Any<GetBooksQuery>()).Returns((books, 2));

        //ACT
        var result = await _sut.GetAllBooksAsync(query);

        //ASSERT
        Assert.Equal(2, result.TotalBooks);

        var mapped = result.Books.ToList();
        Assert.Equal(2, mapped.Count);
        Assert.Equal(books[0].Id, mapped[0].Id);
        Assert.Equal(books[0].Title, mapped[0].Title);
        Assert.Equal(books[0].AuthorId, mapped[0].AuthorId);
        Assert.Equal(books[0].Genre, mapped[0].Genre);
        Assert.Equal(books[0].PublicationYear, mapped[0].PublicationYear);
        Assert.Equal(books[1].Id, mapped[1].Id);
        Assert.Equal(books[1].Title, mapped[1].Title);
        Assert.Equal(books[1].AuthorId, mapped[1].AuthorId);
        Assert.Equal(books[1].Genre, mapped[1].Genre);
        Assert.Equal(books[1].PublicationYear, mapped[1].PublicationYear);

    }


}