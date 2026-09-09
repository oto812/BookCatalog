using BookCatalog.Api.ExceptionHandling;
using BookCatalog.Application.Interfaces;
using BookCatalog.Application.Services;
using BookCatalog.Infrastructure.Persistence;
using BookCatalog.Infrastructure.Repositories;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(8);
});

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
    options.UseUtcTimestamp = true;
});

builder.Logging.Configure(options =>
    options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddDbContext<BookCatalogDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("BookCatalog"),
        npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null)
        );
});
builder.Services.AddScoped<IBookRepository, EfBookRepository>();
builder.Services.AddScoped<ILoanRepository, EfLoanRepository>();
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<BookCatalogDbContext>(name: "database", tags: ["ready"]);

var app = builder.Build();


if (string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("BookCatalog")))
{
    throw new InvalidOperationException(
        "Connection string 'BookCatalog' is missing. Set it in appsettings.json, " +
        "or via the ConnectionStrings__BookCatalog environment variable.");
}

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-Trace-Id"] =
            Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        return Task.CompletedTask;
    });

    await next();
});

app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider
                  .GetRequiredService<BookCatalogDbContext>();

    await db.Database.MigrateAsync();
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Book Catalog API v1");
    });
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false          
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapControllers();

app.Run();

// Exposes the implicit Program class so WebApplicationFactory<Program> can boot the app in tests.
public partial class Program { }
