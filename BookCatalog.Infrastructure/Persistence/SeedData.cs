using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookCatalog.Infrastructure.Persistence
{
    public static class SeedData
    {
        // fixed ids: HasData must be deterministic, or every `migrations add`
        // produces a spurious diff
        static readonly Guid Herbert = new("a1111111-1111-1111-1111-111111111111");
        static readonly Guid King = new("a2222222-2222-2222-2222-222222222222");
        static readonly Guid Asimov = new("a3333333-3333-3333-3333-333333333333");

        static readonly Guid Dune = new("b1111111-1111-1111-1111-111111111111");
        static readonly Guid Messiah = new("b2222222-2222-2222-2222-222222222222");
        static readonly Guid It = new("b3333333-3333-3333-3333-333333333333");
        static readonly Guid Misery = new("b4444444-4444-4444-4444-444444444444");
        static readonly Guid Foundation = new("b5555555-5555-5555-5555-555555555555");

        static readonly Guid Alice = new("c1111111-1111-1111-1111-111111111111");
        static readonly Guid Bob = new("c2222222-2222-2222-2222-222222222222");

        static readonly DateTime Seeded = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static void Apply(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>().HasData(
                new { Id = Herbert, FirstName = "Frank", LastName = "Herbert", BirthDate = new DateOnly(1920, 10, 8) },
                new { Id = King, FirstName = "Stephen", LastName = "King", BirthDate = new DateOnly(1947, 9, 21) },
                new { Id = Asimov, FirstName = "Isaac", LastName = "Asimov", BirthDate = new DateOnly(1920, 1, 2) });

            modelBuilder.Entity<Book>().HasData(
                new { Id = Dune, Title = "Dune", AuthorId = Herbert, PublicationYear = 1965, Genre = Genre.Science, CreatedAt = Seeded, UpdatedAt = Seeded },
                new { Id = Messiah, Title = "Dune Messiah", AuthorId = Herbert, PublicationYear = 1969, Genre = Genre.Science, CreatedAt = Seeded, UpdatedAt = Seeded },
                new { Id = It, Title = "It", AuthorId = King, PublicationYear = 1986, Genre = Genre.Horror, CreatedAt = Seeded, UpdatedAt = Seeded },
                new { Id = Misery, Title = "Misery", AuthorId = King, PublicationYear = 1987, Genre = Genre.Thriller, CreatedAt = Seeded, UpdatedAt = Seeded },
                new { Id = Foundation, Title = "Foundation", AuthorId = Asimov, PublicationYear = 1951, Genre = Genre.Science, CreatedAt = Seeded, UpdatedAt = Seeded });

            modelBuilder.Entity<User>().HasData(
                new { Id = Alice, FirstName = "Alice", LastName = "Doe", Email = "alice@example.com", BirthDate = new DateTime(1995, 3, 14, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = Bob, FirstName = "Bob", LastName = "Smith", Email = "bob@example.com", BirthDate = new DateTime(1990, 7, 2, 0, 0, 0, DateTimeKind.Utc) });

            modelBuilder.Entity<Loan>().HasData(
                // returned - gives the history endpoint something to show
                new
                {
                    Id = new Guid("d1111111-1111-1111-1111-111111111111"),
                    UserId = Alice,
                    BookId = It,
                    BorrowedAt = Seeded,
                    ReturnedAt = (DateTime?)new DateTime(2024, 1, 20, 0, 0, 0, DateTimeKind.Utc)
                },
                // still out - occupies the filtered unique index slot for Foundation
                new
                {
                    Id = new Guid("d2222222-2222-2222-2222-222222222222"),
                    UserId = Bob,
                    BookId = Foundation,
                    BorrowedAt = Seeded,
                    ReturnedAt = (DateTime?)null
                });
        }
    }
}
