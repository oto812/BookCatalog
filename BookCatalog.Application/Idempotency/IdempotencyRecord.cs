namespace BookCatalog.Application.Idempotency
{
    // Remembers that a create request with this key was already handled, and what it created.
    // Not a domain entity: "was this HTTP request seen before?" is not a rule about books.
    public class IdempotencyRecord
    {
        public const int MaxKeyLength = 100;

        // The primary key. The database refuses a second row with the same key, which is what
        // stops two identical requests from both creating a book - even at the same moment.
        public string Key { get; private set; } = null!;

        // A fingerprint of the request body, to tell a retry (same body) from a key reused for a
        // different request.
        public string RequestHash { get; private set; } = null!;

        public Guid BookId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private IdempotencyRecord() { }
        public IdempotencyRecord(string key, string requestHash, Guid bookId)
        {
            Key = key;
            RequestHash = requestHash;
            BookId = bookId;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
