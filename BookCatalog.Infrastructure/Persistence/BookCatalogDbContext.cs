using BookCatalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Infrastructure.Persistence
{
    public class BookCatalogDbContext : DbContext
    {
        public const string ActiveLoanIndex = "UX_Loan_ActiveBook";
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<User> Users { get; set; }

        public BookCatalogDbContext(DbContextOptions<BookCatalogDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>(author =>
            {
                author.HasKey(a => a.Id);
                author.Property(a => a.FirstName).IsRequired().HasMaxLength(100);
                author.Property(a => a.LastName).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<User>(user =>
            {
                user.HasKey(u => u.Id);
                user.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
                user.Property(u => u.LastName).IsRequired().HasMaxLength(100);
                user.Property(u => u.Email).IsRequired().HasMaxLength(150);
                user.HasIndex(u => u.Email).IsUnique();
            });

            modelBuilder.Entity<Book>(book =>
            {
                book.HasKey(b => b.Id);
                book.Property(b => b.Title).IsRequired().HasMaxLength(100);
                book.Property(b => b.Genre).HasConversion<string>().HasMaxLength(50);

                book.HasOne(b => b.Author)
                     .WithMany(a => a.Books)
                     .HasForeignKey(b => b.AuthorId)
                     .OnDelete(DeleteBehavior.Restrict);

                book.HasIndex(b => b.CreatedAt);

                book.ToTable(t => t.HasCheckConstraint(
                    "CK_Book_PublicationYear",
                    "\"PublicationYear\" >= 0 AND \"PublicationYear\" <= EXTRACT(YEAR FROM CURRENT_DATE)"
                    ));

            });


            modelBuilder.Entity<Loan>(loan =>
            {
                loan.HasKey(l => l.Id);

                loan.HasOne(l => l.Book)
                    .WithMany()
                    .HasForeignKey(l => l.BookId)
                    .OnDelete(DeleteBehavior.Restrict);

                loan.HasOne(l => l.User)
                    .WithMany(u => u.Loans)
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                loan.HasIndex(l => l.BookId)
                .IsUnique()
                .HasFilter("\"ReturnedAt\" IS NULL")
                .HasDatabaseName(ActiveLoanIndex);

            });
            SeedData.Apply(modelBuilder);


        }

        
    }
}
