using Anthology.Data;
using Anthology.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Anthology.Api
{
    [Route("Images")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        // GET Images/Person/ANTH368784530868/filename.jpg
        [HttpGet("{category}/{id}/{filename}")]
        public IActionResult Get(string category, string id, string filename)
        {
            var image = _imageService.GetImage(category, id, filename);

            var path = Path.Combine(image.GetPath());
            string mimeType = MimeMapping.MimeUtility.GetMimeMapping(filename);

            Byte[] b = System.IO.File.ReadAllBytes(path);
            return File(b, mimeType);

        }

        // GET Images/Book/ANTH368784530868/BookCovers/filename.jpg
        [HttpGet("{category}/{id}/{subcategory}/{filename}")]
        public IActionResult Get(string category, string id, string filename, string subcategory)
        {
            var image = _imageService.GetImage(category, id, filename, subcategory);

            var path = Path.Combine(image.GetPath());
            string mimeType = MimeMapping.MimeUtility.GetMimeMapping(filename);

            Byte[] b = System.IO.File.ReadAllBytes(path);
            return File(b, mimeType);

        }
    }
}
