using Anthology.Data.Metadata;
using Anthology.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Anthology.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetadataController : ControllerBase
    {
        // GET api/Metadata/Audiobook/ANTH368784530868
        [HttpGet("Audiobook/{isbn}")]
        public Book? GetAudiobook(string isbn)
        {
            return MetadataService.GetBookMetadata(isbn, true).Result;
        }

        // GET api/Metadata/Book/ANTH368784530868
        [HttpGet("Book/{isbn}")]
        public Book? GetBook(string isbn)
        {
            return MetadataService.GetBookMetadata(isbn, false).Result;
        }

        // GET api/Metadata/Audiobook/Search
        [HttpGet("Audiobook/Search")]
        public List<Book> AudiobookSearch(string title, string? author = null, string? isbn = null, string? asin = null)
        {
            return MetadataService.Search(title, author, isbn, asin, true).Result;
        }

        // GET api/Metadata/Book/Search
        [HttpGet("Book/Search")]
        public List<Book> BookSearch(string title, string? author = null, string? isbn = null, string? asin = null)
        {
            return MetadataService.Search(title, author, isbn, asin, false).Result;
        }
    }
}
