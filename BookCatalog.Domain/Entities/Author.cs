namespace BookCatalog.Domain.Entities
{
    public class Author
    {
        public Guid Id { get; private set; }
        public string FirstName { get; private set; } = null!;
        public string LastName { get; private set; } = null!;
        public DateOnly BirthDate { get; private set; }
        public ICollection<Book> Books { get; private set; } = new List<Book>();


        private Author() { }

        public Author (string firstName, string lastName, DateOnly birthDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (birthDate > today) throw new ArgumentOutOfRangeException(nameof(BirthDate), "BirthDate cannot be in the future");
            if(string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("FirstName is required", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("lastName is required", nameof(lastName));
            Id = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            BirthDate = birthDate;
        }
    }
}
