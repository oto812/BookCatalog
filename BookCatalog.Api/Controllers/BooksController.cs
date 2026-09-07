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
            [FromQuery] GetBooksQuery getBooksQuery, CancellationToken cancellationToken)
        {
            var pagedResponse = await _bookService.GetAllBooksAsync(getBooksQuery, cancellationToken);

            return Ok(pagedResponse);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookResponse>> GetBookById(Guid id, CancellationToken cancellationToken)
        {
            var response = await _bookService.GetBookByIdAsync(id, cancellationToken);
            if (response == null)
            {
                return NotFound();
            }
            return response;
        }

        [HttpPost]
        public async Task<ActionResult<BookResponse>> AddBook(CreateBookRequest createBookRequest, CancellationToken cancellationToken)
        {
            var book = await _bookService.AddBookAsync(createBookRequest, cancellationToken);
            return CreatedAtAction(nameof(GetBookById), new { id = book!.Id }, book);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BookResponse>> UpdateBook(UpdateBookRequest updateBookDto, Guid id, CancellationToken cancellationToken)
        {
            var book = await _bookService.UpdateBookAsync(updateBookDto, id, cancellationToken);
            if (book == null) { 
                return NotFound();
            }
            else
            {
                return book;
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(Guid id, CancellationToken cancellationToken)
        {
            var deleted = await _bookService.DeleteBookAsync(id, cancellationToken);

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
