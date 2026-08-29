using BookCatalog.Domain.Enums;


namespace BookCatalog.Domain.Entities
{
    public class Book
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; } = null!;
        public int PublicationYear { get; private set; } 
        public Genre Genre { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public Guid AuthorId { get; private set; }
        public Author Author { get; private set; } = null!;

        private Book() { }
        public Book(string title, Guid authorId, int publicationYear, Genre genre){
            var creationTime = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required", nameof(title));
            if (authorId == Guid.Empty) throw new ArgumentException("AuthorId is required", nameof(AuthorId));
            if (publicationYear > DateTime.UtcNow.Year || publicationYear < 0) throw new ArgumentOutOfRangeException(nameof(publicationYear));
            Title = title;
            PublicationYear = publicationYear;
            Genre = genre;
            CreatedAt = creationTime;
            UpdatedAt = creationTime;
            Id = Guid.NewGuid();
            AuthorId = authorId;
        }
           

        private Book(Guid id, string title, Guid authorId, int publicationYear, Genre genre, DateTime createdAt)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required", nameof(title));
            if (authorId == Guid.Empty) throw new ArgumentException("AuthorId is required", nameof(AuthorId));
            if (publicationYear > DateTime.UtcNow.Year || publicationYear < 0) throw new ArgumentOutOfRangeException(nameof(publicationYear));
            Title = title;
            PublicationYear = publicationYear;
            Genre = genre;
            CreatedAt = createdAt;
            UpdatedAt = DateTime.UtcNow;
            Id = id;
            AuthorId = authorId;
        }

        public Book Update(string title, Guid authorId, int publicationYear, Genre genre)
        {
            return new Book(Id, title, authorId, publicationYear, genre, CreatedAt);
        }


    }
}
