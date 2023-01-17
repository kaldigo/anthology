using Anthology.Data;
using Microsoft.AspNetCore.Components.Forms;

namespace Anthology.Services
{
    public interface IImageService
    {
        void DeleteImage(Image image);
        Image GetImage(Guid guid);
        Image? GetImage(string category, string id, string filename, string subcategory = null);
        Task<TempImage> SaveTempImage(IBrowserFile image);

    }
}