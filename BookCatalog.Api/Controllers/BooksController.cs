using BookCatalog.Application.DTOs;
using BookCatalog.Application.Services;
using BookCatalog.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
        public ActionResult<PagedBooksResponse> GetAllBooks(
            [FromQuery] string? author, 
            [FromQuery] Genre? genre, 
            [FromQuery] int? publicationYear,
            [FromQuery][Range(1, int.MaxValue)] int page = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10)
        {
            var pagedResponse = _bookService.GetAllBooks(author, genre, publicationYear, page, pageSize);

            return Ok(pagedResponse);

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
