using BookCatalog.Domain.Entities;

namespace BookCatalog.Application.Interfaces
{
    public interface IBookRepository
    {
        Book? GetById(Guid id);
        IEnumerable<Book> GetAll();
        Book? AddBook(Book book);
    }
}
