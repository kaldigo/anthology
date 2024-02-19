using Anthology.Data;
using Anthology.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Anthology.Services.MetadataService;

namespace Anthology.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetadataController : ControllerBase
    {
        private readonly IBookService _bookService;
        public MetadataController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // GET api/Metadata/Search
        [HttpGet("Search")]
        public List<ApiMetadata> AudiobookSearch(string query, string? author = null, string? isbn = null)
        {
            var parameters = new Dictionary<string, string>();
            if (query != null) parameters.Add("title", query);
            if (author != null) parameters.Add("author", author);
            if (isbn != null) parameters.Add("isbn", isbn);
            parameters.Add("host", this.Request.Scheme + "://" + this.Request.Host);
            return _bookService.Search(parameters).Result;
        }
    }
}
