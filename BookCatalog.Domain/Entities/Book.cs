using BookCatalog.Domain.Enums;


namespace BookCatalog.Domain.Entities
{
    public class Book
    {
        public Guid Id { get; private set; } 
        public string Title { get; private set; }
        public string Author { get; private set; }
        public int PublicationYear { get; private set; }
        public Genre Genre { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public Book(string title, string author, int publicationYear, Genre genre) {
            if(title == null) throw new ArgumentNullException("title");
            if(author == null) throw new ArgumentNullException("author");
            if(publicationYear > DateTime.Now.Year || publicationYear < 0) throw new ArgumentOutOfRangeException("publicationYear", "Publication year cannot be in the future or a negative number");
            Title = title;
            Author = author;
            PublicationYear = publicationYear;
            Genre = genre;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }


    }
}
