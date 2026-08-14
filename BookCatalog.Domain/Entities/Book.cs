using BookCatalog.Domain.Enums;


namespace BookCatalog.Domain.Entities
{
    public class Book
    {
        public Guid Id { get; set; } 
        public string Title { get; set; }
        public string Author { get; set; }
        public int PublicationYear { get; set; }
        public Genre Genre { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
