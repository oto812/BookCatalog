using BookCatalog.Application.DTOs;
using BookCatalog.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<BookResponse>> GetAllBooks()
        {
            var books = _bookService.GetAllBooks();

            return Ok(books);

        }

        [HttpGet("{id}")]
        public ActionResult<BookResponse> GetBookById(Guid id)
        {
            var response = _bookService.GetBookById(id);
            if (response == null)
            {
                return NotFound();
            }
            return response;
        }

        [HttpPost]
        public ActionResult<BookResponse> AddBook(CreateBookRequest createBookRequest)
        {
            var book = _bookService.AddBook(createBookRequest);
            if (book == null) {
                 return Conflict();
            }

            return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
        }

        [HttpPut("{id}")]
        public ActionResult<BookResponse> UpdateBook(UpdateBookRequest updateBookDto, Guid id)
        {
            var book = _bookService.UpdateBook(updateBookDto, id);
            if (book == null) { 
                return NotFound();
            }
            else
            {
                return book;
            }
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteBook(Guid id)
        {
            var deleted = _bookService.DeleteBook(id);

            if (!deleted) {
                return NotFound();
            }
            else
            {
                return NoContent();
            }
        }
        


    }
}
