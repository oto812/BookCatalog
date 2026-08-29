using BookCatalog.Api.ExceptionHandling;
using BookCatalog.Application.Interfaces;
using BookCatalog.Application.Services;
using BookCatalog.Infrastructure.Persistence;
using BookCatalog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddDbContext<BookCatalogDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("BookCatalog")).LogTo(Console.WriteLine, LogLevel.Information);
});
builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();
var app = builder.Build();

app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Book Catalog API v1");
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
