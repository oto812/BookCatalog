using BookCatalog.Application.Interfaces;
using BookCatalog.Application.Services;
using BookCatalog.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();
var app = builder.Build();

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
