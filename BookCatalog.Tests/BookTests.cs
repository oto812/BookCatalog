using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Enums;


namespace BookCatalog.Tests
{
    public class BookTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(2500)]
        public void Constructor_Throws_WhenPublicationYearIsInvalid(int year)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Book("Dune", Guid.NewGuid(), year, Genre.Fantasy));
        }

        [Fact]
        public void Constructor_Throws_WhenTitleIsNull()
        {
            Assert.Throws<ArgumentException>(
            
                () => new Book(null!, Guid.NewGuid(), 2000, Genre.Fantasy));
        }
        [Fact]
        public void Constructor_Throws_WhenAuthorIdIsEmpty()
        {
            Assert.Throws<ArgumentException>(

                () => new Book("Dune", Guid.Empty, 2000, Genre.Fantasy));
        }

        //[Fact]
        //public void Update_PreservesIdAndCreatedAt() { 
        //    var book = new Book("Dune", Guid.NewGuid(), 2000, Genre.Fantasy);
        //    var updatedBook = book.Update("Duke", Guid.NewGuid(), 2010, Genre.Fantasy);
        //    Assert.Equal(updatedBook.Id, book.Id);
        //    Assert.Equal(updatedBook.CreatedAt, book.CreatedAt);
        //}

        //[Fact]
        //public void Update_ReturnsUpdatedBook_WhenValidParametersProvided()
        //{
        //    var book = new Book("Dune", Guid.NewGuid(), 2000, Genre.Fantasy);
        //    var newAuthorId = Guid.NewGuid();
        //    var updatedBook = book.Update("Duke", newAuthorId, 2010, Genre.Science);

        //    Assert.Equal("Duke", updatedBook.Title);
        //    Assert.Equal(newAuthorId, updatedBook.AuthorId);
        //    Assert.Equal(2010, updatedBook.PublicationYear);
        //    Assert.Equal(Genre.Science,updatedBook.Genre);

        //}

        [Theory]
        [InlineData(-1)]
        [InlineData(2500)]
        public void Update_Throws_WhenPublicationYearIsInvalid(int years)
        {
            var book = new Book("Dune", Guid.NewGuid(), 2000, Genre.Fantasy);
            Assert.Throws<ArgumentOutOfRangeException>(
                () => book.Update("Duke", Guid.NewGuid(), years, Genre.Fantasy));
        }

        [Fact]
        public void Update_Throws_WhenTitleIsNull()
        {
            var book = new Book("Dune", Guid.NewGuid(), 2000, Genre.Fantasy);
            Assert.Throws<ArgumentException>(
                () => book.Update(null!, Guid.NewGuid(), 2000, Genre.Fantasy));
        }

        [Fact]
        public void Update_Throws_WhenAuthorIdIsEmpty()
        {
            var book = new Book("Dune", Guid.NewGuid(), 2000, Genre.Fantasy);
            Assert.Throws<ArgumentException>(
                () => book.Update("Duke", Guid.Empty, 2000, Genre.Fantasy));
        }

        [Fact]
        public void Update_UpdatesTheFields_WhenUpdateSucceeds()
        {
            var book = new Book("Dune", Guid.NewGuid(), 2000, Genre.Fantasy);
            var beforeUpdateDate = book.UpdatedAt;
            book.Update("Duke", Guid.NewGuid(), 2010, Genre.Science);
            var afterUpdateDate = book.UpdatedAt;


            Assert.Equal("Duke", book.Title);
            Assert.Equal(2010, book.PublicationYear);
            Assert.Equal(Genre.Science, book.Genre);
            Assert.True(afterUpdateDate > beforeUpdateDate);


        }

    }
}
