using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebUI.Models
{
    public class Catalog
    {
        public string Name { get; set; }
        public string PathName { get; set; }
        public double BusySpace { get; set; }
        public string Image { get; set; }

        // доступ к файлу закрыт
        public bool NoAccessToFile { get; set; }
    }
}
