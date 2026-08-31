using BookCatalog.Application.DTOs.Queries;
using BookCatalog.Application.DTOs.Requests;
using BookCatalog.Application.DTOs.Responses;
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
        public async Task<ActionResult<PagedBooksResponse>> GetAllBooks(
            [FromQuery] GetBooksQuery getBooksQuery)
        {
            var pagedResponse = await _bookService.GetAllBooksAsync(getBooksQuery);

            return Ok(pagedResponse);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookResponse>> GetBookById(Guid id)
        {
            var response = await _bookService.GetBookByIdAsync(id);
            if (response == null)
            {
                return NotFound();
            }
            return response;
        }

        [HttpPost]
        public async Task<ActionResult<BookResponse>> AddBook(CreateBookRequest createBookRequest)
        {
            var book = await _bookService.AddBookAsync(createBookRequest);
            if (book == null) {
                 return Conflict();
            }

            return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BookResponse>> UpdateBook(UpdateBookRequest updateBookDto, Guid id)
        {
            var book = await _bookService.UpdateBookAsync(updateBookDto, id);
            if (book == null) { 
                return NotFound();
            }
            else
            {
                return book;
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(Guid id)
        {
            var deleted = await _bookService.DeleteBookAsync(id);

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
