using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebUI.Models
{
    public class Disk
    {
        public string Name { get; set; }
        public string NamePath { get; set; }
        public double Space { get; set; }
        public double FreeSpace { get; set; }
        public string Image { get; set; }
    }
}
