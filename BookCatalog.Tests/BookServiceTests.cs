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
    public void GetBookById_ReturnsNull_WhenBookDoesNotExist()
    {
        // ARRANGE 
        var id = Guid.NewGuid();
        _repository.GetById(id).Returns((Book?)null);

        // ACT
        var result = _sut.GetBookById(id);

        // ASSERT 
        Assert.Null(result);
    }

    [Fact]
    public void GetBookById_ReturnsMappedResponse_WhenBookExists()
    {
        // ARRANGE 
        var book = new Book("Dune", "Frank Herbert", 1965, Genre.Fantasy);
        _repository.GetById(book.Id).Returns(book);

        // ACT
        var result = _sut.GetBookById(book.Id);

        // ASSERT 
        Assert.NotNull(result);
        Assert.Equal(book.Id, result.Id);
        Assert.Equal("Dune", result.Title);
        Assert.Equal("Frank Herbert", result.Author);
        Assert.Equal(1965, result.PublicationYear);
        Assert.Equal(Genre.Fantasy, result.Genre);
    }

    [Fact]
    public void DeleteBook_ReturnsFalse_WhenBookDoesNotExist()
    {
        // ARRANGE
        var id = Guid.NewGuid();
        _repository.DeleteBookById(id).Returns(false);

        // ACT
        var result = _sut.DeleteBook(id);

        // ASSERT 
        Assert.False(result);

       
    }

    [Fact]
    public void DeleteBook_ReturnsTrue_WhenBookExists()
    {
        // ARRANGE
        var id = Guid.NewGuid();
        _repository.DeleteBookById(id).Returns(true);
        // ACT

        var result = _sut.DeleteBook(id);

        //ASSERT
        Assert.True(result);
    }

    [Fact]
    public void AddBook_ReturnsNull_WhenRepositoryFails()
    {
        // ARRANGE
        var request = new CreateBookRequest("Dune", "Frank Herbert", 1965, Genre.Fantasy);
        _repository.AddBook(Arg.Any<Book>()).Returns((Book?)null);
        // ACT
        var result = _sut.AddBook(request);
        // ASSERT
        Assert.Null(result);
    }

    [Fact]
    public void AddBook_ReturnsResponse_WhenRepositorySucceeds()
    {
        // ARRANGE
        var request = new CreateBookRequest("Dune", "Frank Herbert", 1965, Genre.Fantasy);
        var book = new Book(request.Title, request.Author, request.PublicationYear, request.Genre);
        _repository.AddBook(Arg.Any<Book>()).Returns(callInfo => callInfo.Arg<Book>());
        // ACT
        var result = _sut.AddBook(request);
        // ASSERT
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(book.Title, result.Title);
        Assert.Equal(book.Author, result.Author);
        Assert.Equal(book.PublicationYear, result.PublicationYear);
        Assert.Equal(book.Genre, result.Genre);
    }

    [Fact]
    public void UpdateBook_ReturnsNull_WhenBookDoesNotExist()
    {
        // ARRANGE
        var id = Guid.NewGuid();
        var request = new UpdateBookRequest("Dune", "Frank Herbert", 1965, Genre.Fantasy);
        _repository.GetById(id).Returns((Book?)null);

        //ACT
        var result = _sut.UpdateBook(request, id);

        //ASSERT
        Assert.Null(result);
    }

    [Fact]
    public void UpdateBook_ReturnsNull_WhenConcurrentUpdateFails()
    {
        // ARRANGE
        var id = Guid.NewGuid();
        var request = new UpdateBookRequest("Dune", "Frank Herbert", 1965, Genre.Fantasy);
        var existingBook = new Book("Old Title", "Old Author", 1900, Genre.Fantasy);
        _repository.GetById(id).Returns(existingBook);

        _repository.UpdateBook(Arg.Any<Book>(), Arg.Any<Book>(), id).Returns((Book?)null);
        //Act
        var result = _sut.UpdateBook(request, id);

        //ASSERT
        Assert.Null(result);
    }
    [Fact]
    public void UpdateBook_ReturnsUpdatedResponse_WhenUpdateSucceeds()
    {
        // ARRANGE
        
        var request = new UpdateBookRequest("New Title", "New Author", 2000, Genre.Science);
        var existingBook = new Book("Old Title", "Old Author", 1900, Genre.Fantasy);
        var id = existingBook.Id;
        _repository.GetById(id).Returns(existingBook);

        _repository.UpdateBook(Arg.Any<Book>(), Arg.Any<Book>(), id)
            .Returns(callInfo => callInfo.ArgAt<Book>(0));

        // ACT
        var result = _sut.UpdateBook(request, id);

        // ASSERT 
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("New Title", result.Title);
        Assert.Equal("New Author", result.Author);
        Assert.Equal(2000, result.PublicationYear);
        Assert.Equal(Genre.Science, result.Genre);
    }

    [Fact]
    public void GetAllBooks_ReturnsPagedResponse()
    {
        //ARRANGE
        var books = new List<Book>
    {
        new Book("Dune", "Frank Herbert", 1965, Genre.Fantasy),
        new Book("It", "Stephen King", 1986, Genre.Horror)
    };
        var query = new GetBooksQuery(null, null, null, 1, 10);
        _repository.GetAll(Arg.Any<GetBooksQuery>()).Returns((books, 2));

        //ACT
        var result = _sut.GetAllBooks(query);

        //ASSERT
        Assert.Equal(2, result.TotalBooks);

        var mapped = result.Books.ToList();
        Assert.Equal(2, mapped.Count);
        Assert.Equal(books[0].Id, mapped[0].Id);
        Assert.Equal(books[0].Title, mapped[0].Title);
        Assert.Equal(books[0].Author, mapped[0].Author);
        Assert.Equal(books[0].Genre, mapped[0].Genre);
        Assert.Equal(books[0].PublicationYear, mapped[0].PublicationYear);
        Assert.Equal(books[1].Id, mapped[1].Id);
        Assert.Equal(books[1].Title, mapped[1].Title);
        Assert.Equal(books[1].Author, mapped[1].Author);
        Assert.Equal(books[1].Genre, mapped[1].Genre);
        Assert.Equal(books[1].PublicationYear, mapped[1].PublicationYear);

    }


}