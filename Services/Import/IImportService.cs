using Anthology.Plugins.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Services
{
    public interface IImportService
    {
        public Task<List<ImportItem>> GetImportList();
    }
}
