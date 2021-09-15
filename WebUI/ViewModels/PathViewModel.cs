using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebUI.Models;

namespace WebUI.ViewModels
{
    public class PathViewModel
    {
        public string Path { get; set; }
        public List<Catalog> Catalogs { get; set; }
        public List<_File> Files { get; set; }
    }
}
