using BookCatalog.Domain.Entities;
using BookCatalog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookCatalog.IntegrationTests.TestHelper
{
    public static class UserHelper
    {
        public static async Task<Guid> GiveAUserAsync(WebApplicationFactory<Program> factory)
        {
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BookCatalogDbContext>();

            var user = new User
            (
                "FirstName",
                "LastName",
                $"{Guid.NewGuid()}@test.com",
                DateTime.UtcNow.AddYears(-20)
            );
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            return user.Id;

        }
    }
}
