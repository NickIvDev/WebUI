using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebUI.Models
{
    public abstract class ForModel
    {
        // имя (для киента)
        public string Name { get; set; }

        // имя дирректории
        public string PathName { get; set; }

        // занимаемое место на диске(либо размер диска - для дисков)
        public double SizeElement { get; set; }

        // ссылка на изображение
        public string Image { get; set; }       

        // доступ к файлу закрыт
        public bool NoAccessToFile { get; set; }
    }
}
