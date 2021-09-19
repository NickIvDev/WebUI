using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebUI.Controllers;

namespace WebUI.Models
{
    public class CatalogModel : ForModel
    {
        public object GetSizeElement(CatalogModel catalog)
        {            
            // рекурсия - получение размера каталогов           
            double sizeCatalog = 0;
            //sizeCatalog = HomeController.SizeCatalog2(catalog.PathName);
            sizeCatalog = HomeController.SizeCatalog(catalog.PathName, ref sizeCatalog);
            
            // для более точного отслеживания нуля
            double testSizeCatalog = sizeCatalog;

            // отлавливаем каталоги без доступа(или с файлами без доступа)
            if (testSizeCatalog == 0)
            {
                try
                {
                    List<string> c = Directory.GetDirectories(catalog.PathName).ToList();
                    List<string> f = Directory.GetFiles(catalog.PathName).ToList();
                    if (c.Count() != 0 || f.Count() != 0)
                    {
                        catalog.NoAccessToFile = true;
                    }
                }
                catch (Exception ex)
                {
                    catalog.NoAccessToFile = true;
                }
            }

            if (testSizeCatalog >= 1024 * 1024 * 1024)
            {
                catalog.TypeOfSize = "GB";
                catalog.SizeElement = Math.Round((double)(sizeCatalog / 1024 / 1024 / 1024), 2);
                catalog.SizeElementToKb = Math.Round((double)(sizeCatalog / 1024), 2);
            }
            else if (testSizeCatalog >= 1024 * 1024)
            {
                catalog.TypeOfSize = "MB";
                catalog.SizeElement = Math.Round((double)(sizeCatalog / 1024 / 1024), 2);
                catalog.SizeElementToKb = Math.Round((double)(sizeCatalog / 1024), 2);
            }
            else
            {
                catalog.TypeOfSize = "KB";
                catalog.SizeElement = Math.Round((double)(sizeCatalog / 1024), 2);
                catalog.SizeElementToKb = Math.Round((double)(sizeCatalog / 1024), 2);
            }
            return default;
        }        
    }
}
