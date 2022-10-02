using Anthology.Data.DB;
using Anthology.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Anthology.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly DatabaseContext _context;
        public BookController (DatabaseContext context)
        {
            this._context = context;
        }

        // GET api/Book/
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var devs = await _context.Books.ToListAsync();
            return Ok(devs);
        }

        // GET api/Book/ANTH368784530868
        [HttpGet("Book/{isbn}")]
        public async Task<IActionResult> Get(string isbn)
        {
            var book = await _context.Books.FirstOrDefaultAsync(a => a.ISBN == isbn);
            return Ok(book);
        }

        // POST api/Book/
        [HttpPost]
        public async Task<IActionResult> Post(Book book)
        {
            _context.Add(book);
            await _context.SaveChangesAsync();
            return Ok(book.ISBN);
        }

        // PUT api/Book/
        [HttpPut]
        public async Task<IActionResult> Put(Book book)
        {
            _context.Entry(book).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/Book/ANTH368784530868
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string isbn)
        {
            var book = new Book { ISBN = isbn };
            _context.Remove(book);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
