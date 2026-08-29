namespace BookCatalog.Domain.Entities
{
    public class Loan
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public User User { get; private set; } = null!;
        public Guid BookId { get; private set; }
        public Book Book { get; private set; } = null!;
        public DateTime BorrowedAt {  get; private set; }
        public DateTime? ReturnedAt { get; private set; }

        private Loan() { }
        public Loan(Guid userId, Guid bookId){
            if (userId == Guid.Empty) throw new ArgumentException("UserId is required", nameof(userId));
            if (bookId == Guid.Empty) throw new ArgumentException("BookId is required", nameof(bookId));
            Id = Guid.NewGuid();
            UserId = userId;
            BookId = bookId;
            BorrowedAt = DateTime.UtcNow;
        }
        public void Return() {
            if (ReturnedAt != null) throw new InvalidOperationException("Already returned");
            ReturnedAt = DateTime.UtcNow;
        }
    }
}
