using Anthology.Data.DB;
using Anthology.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Anthology.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly DatabaseContext _context;
        public ImageController(DatabaseContext context)
        {
            this._context = context;
        }

        // GET api/Image/ANTH368784530868/BookCovers/filename.jpg
        [HttpGet("{isbn}/{category}/{filename}")]
        public IActionResult Get(string isbn, string category, string filename)
        {
            string folderName = "Book Covers";
            switch (category)
            {
                case "AudiobookCovers":
                    folderName = "Audiobook Covers";
                    break;
                case "ExtraImages":
                    folderName = "Extra Images";
                    break;
            }

            var path = Path.Combine(Utils.FileUtils.GetConfigPath(), "Images", isbn, folderName, filename);
            string mimeType = MimeMapping.MimeUtility.GetMimeMapping(filename);

            Byte[] b = System.IO.File.ReadAllBytes(path);
            return File(b, mimeType);

        }
    }
}
