using BookCatalog.Domain.Entities;
using BookCatalog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;


namespace BookCatalog.IntegrationTests.TestHelper
{
    public static class AuthorHelper
    {
        public static async Task<Guid> GiveAnAuthorAsync(WebApplicationFactory<Program> factory)
        {
            using var scope = factory.Services.CreateScope();
            var dbcontext = scope.ServiceProvider.GetRequiredService<BookCatalogDbContext>();

            var author = new Author(
            "Frank",
            "Herbert",
            new DateOnly(1920, 10, 8));
            dbcontext.Authors.Add(author);
            await dbcontext.SaveChangesAsync();
            return author.Id;
        }
    }
}
