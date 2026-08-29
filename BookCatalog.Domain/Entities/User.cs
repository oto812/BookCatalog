
namespace BookCatalog.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string FirstName { get; private set; } = null!;
        public string LastName { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public DateTime BirthDate { get; private set; } 

        private User() { }
        public User(string firstName, string lastName, string email, DateTime birthDate)
        {
            if (birthDate > DateTime.UtcNow) throw new ArgumentOutOfRangeException(nameof(BirthDate), "BirthDate cannot be in the future");
            if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("FirstName is required", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("lastName is required", nameof(lastName));
            if(string.IsNullOrWhiteSpace(email) || !email.Contains("@")) throw new ArgumentException("Email is invalid", nameof(email));
            Id = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            BirthDate = birthDate;
        }


    }
}
