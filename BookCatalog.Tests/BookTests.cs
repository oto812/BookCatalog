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
                () => new Book("Dune", "Frank Herbert", year, Genre.Fantasy));
        }

        [Fact]
        public void Constructor_Throws_WhenTitleIsNull()
        {
            Assert.Throws<ArgumentNullException>(
            
                () => new Book(null, "Frank Herbert", 2000, Genre.Fantasy));
        }
        [Fact]
        public void Constructor_Throws_WhenAuthorIsNull()
        {
            Assert.Throws<ArgumentNullException>(

                () => new Book("Dune", null, 2000, Genre.Fantasy));
        }

        [Fact]
        public void Update_PreservesIdAndCreatedAt() { 
            var book = new Book("Dune", "Frank Herbert", 2000, Genre.Fantasy);
            var updatedBook = book.Update("Duke", "author2", 2010, Genre.Fantasy);
            Assert.Equal(updatedBook.Id, book.Id);
            Assert.Equal(updatedBook.CreatedAt, book.CreatedAt);
        }

        [Fact]
        public void Update_ReturnsUpdatedBook_WhenValidParametersProvided()
        {
            var book = new Book("Dune", "Frank Herbert", 2000, Genre.Fantasy);
            var updatedBook = book.Update("Duke", "author2", 2010, Genre.Science);

            Assert.Equal("Duke", updatedBook.Title);
            Assert.Equal("author2", updatedBook.Author);
            Assert.Equal(2010, updatedBook.PublicationYear);
            Assert.Equal(Genre.Science,updatedBook.Genre);

        }


    }
}
