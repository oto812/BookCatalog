using BookCatalog.Application.DTOs;
using BookCatalog.Application.Interfaces;
using BookCatalog.Domain.Entities;


namespace BookCatalog.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;

        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public BookResponse? AddBook(CreateBookRequest createBookRequest)
        {
            var book = new Book(
                createBookRequest.Title,
                createBookRequest.Author,
                createBookRequest.PublicationYear,
                createBookRequest.Genre
            );
            var result = _bookRepository.AddBook(book);
            if(result == null)
            {
                return null;
            }
            return new BookResponse(
                book.Id,
                book.Title,
                book.Author,
                book.Genre
            );

        }

        public bool DeleteBook(Guid id)
        {
            var success = _bookRepository.DeleteBookById(id);
            return success;

        }

        public IEnumerable<BookResponse> GetAllBooks()
        {
               var books = _bookRepository.GetAll();
               return books.Select(book => new BookResponse
               (
                   book.Id,
                   book.Title,
                   book.Author,
                   book.Genre
               ));
        }

        

        public BookResponse? GetBookById(Guid id)
        {
            var book =  _bookRepository.GetById(id);
            if (book == null) return null;
            return new BookResponse
            (
                book.Id,
                book.Title,
                book.Author,
                book.Genre
            );
        }

        public BookResponse? UpdateBook(UpdateBookRequest updateBookDto, Guid id)
        {
            var oldBook = _bookRepository.GetById(id);
            
            if (oldBook == null) return null;
            var newBook = oldBook.Update(updateBookDto.Title, updateBookDto.Author, updateBookDto.PublicationYear, updateBookDto.Genre);

            var book = _bookRepository.UpdateBook(newBook, oldBook, id);
            
            if (book == null)
            {
                return null;
            }
            return new BookResponse ( id, book.Title, book.Author, book.Genre );
        }
    }
}
