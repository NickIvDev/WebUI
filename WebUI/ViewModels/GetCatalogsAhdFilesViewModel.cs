using System.Collections.Generic;
using WebUI.Models;

namespace WebUI.ViewModels
{
    public class GetCatalogsAhdFilesViewModel
    {
        public string Path { get; set; }
        public List<CatalogModel> Catalogs { get; set; }
        public List<FileModel> Files { get; set; }
    }
}
