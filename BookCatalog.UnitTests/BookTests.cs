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
            var originalId = book.Id;
            var originalCreatedAt = book.CreatedAt;
            var newAuthorId = Guid.NewGuid();
            var beforeUpdateDate = book.UpdatedAt;

            book.Update("Duke", newAuthorId, 2010, Genre.Science);
            var afterUpdateDate = book.UpdatedAt;


            Assert.Equal("Duke", book.Title);
            Assert.Equal(2010, book.PublicationYear);
            Assert.Equal(Genre.Science, book.Genre);
            // AuthorId is asserted because it is the field Update once silently dropped:
            // the guards were duplicated across two constructors and the copies drifted.
            Assert.Equal(newAuthorId, book.AuthorId);
            Assert.True(afterUpdateDate > beforeUpdateDate);

            // Identity survives an update - a revised book is the same book.
            Assert.Equal(originalId, book.Id);
            Assert.Equal(originalCreatedAt, book.CreatedAt);


        }

    }
}
