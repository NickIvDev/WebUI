using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebUI.Models;
using WebUI.ViewModels;

namespace WebUI.Controllers
{   
    public class HomeController : Controller
    {        
        [HttpGet]
        public IActionResult GetPcDisks() // просмотр дисков ПК
        {
            // получаем диски с ПК
            List<DriveInfo> pcDisks = DriveInfo.GetDrives().ToList();

            // инициализируем модели Disk
            List<DiskModel> disks = new List<DiskModel>();
            foreach (var item in pcDisks)
            {
                // присваеваем нужную ссылку на изображение
                string image;
                if (item.Name.StartsWith("C:"))
                {
                    image = "/images/systemDisk.jpg";
                }
                else
                    image = "/images/localDick.jpg";

                disks.Add(new DiskModel
                {
                    Name = item.Name.Trim('\\'),
                    PathName = item.Name,
                    SizeElement = Math.Round((double)item.TotalSize / 1024 / 1024 / 1024, 2),
                    FreeSpace = Math.Round((double)item.TotalFreeSpace / 1024 / 1024 / 1024, 2),
                    Image = image
                });
            }
            return View(disks);
        }

        [HttpGet]
        public IActionResult TransitionToCatalog(string path) // просмотр переданного каталога
        {
            GetCatalogsAhdFilesViewModel model = GetDataViewModel(path);

            // отлавливаем каталоги без доступа
            if (model==null)
            {
                return View("NoAccessToFile");
            }
            else
                return View("GetCatalogsAhdFiles", model);
        }

        [HttpPost]
        public IActionResult GetFilterDataByName(string path, string elementName) // фильтрация переданного каталога по имени
        {
            GetCatalogsAhdFilesViewModel tempModel = GetDataViewModel(path);

            // для возвращения всего списка
            if (elementName == null)
            {
                return View("GetCatalogsAhdFiles", tempModel);
            }
            else // фильтрация списка по имени 
            {
                GetCatalogsAhdFilesViewModel model = new GetCatalogsAhdFilesViewModel
                {
                    Path = path,
                    Catalogs = tempModel.Catalogs.Where(c => c.Name.Contains(elementName)).ToList(),
                    Files = tempModel.Files.Where(f => f.Name.Contains(elementName)).ToList()
                };
                return View("GetCatalogsAhdFiles", model);
            }          
        }
        
        static GetCatalogsAhdFilesViewModel GetDataViewModel(string path) // получение модели представления по указанному пути
        {
            try
            {
                // получаем файлы и каталоги дирректории
                List<string> pcCatalogs = Directory.GetDirectories(path).ToList();
                List<string> pcFiles = Directory.GetFiles(path).ToList();

                // получаем экземпляры FileInfo для получения занимаегого места
                // файлы:
                List<FileInfo> fileInfosForFiles = new List<FileInfo>();
                foreach (var item in pcFiles)
                {
                    fileInfosForFiles.Add(new FileInfo(item));
                }

                // инициализируем модели Catalog и File
                List<CatalogModel> catalogs = new List<CatalogModel>();
                List<FileModel> files = new List<FileModel>();

                // каталоги
                foreach (var item in pcCatalogs)
                {
                    int lastIndex;
                    if (item.Contains('/'))
                    {
                        lastIndex = item.LastIndexOf('/');
                    }
                    else
                        lastIndex = item.LastIndexOf('\\');

                    string name = item.Substring(++lastIndex);
                    catalogs.Add(new CatalogModel
                    {
                        Name = name,
                        PathName = item, //= tempData,
                        Image = "/images/folder.jpg",
                        NoAccessToFile = false
                    });

                    
                }
                
                // файлы
                foreach (var item in pcFiles)
                {
                    int lastIndex;
                    if (item.Contains('/'))
                    {
                        lastIndex = item.LastIndexOf('/');
                    }
                    else
                        lastIndex = item.LastIndexOf('\\');

                    FileInfo toBusySpace = fileInfosForFiles.FirstOrDefault(f => f.FullName == item);
                    double sizeElement = toBusySpace.Length;
                    double sizeElementToKb = toBusySpace.Length;
                    string typeOfSize;
                    if (sizeElement >= 1024 * 1024 * 1024)
                    {
                        typeOfSize = "GB";
                        sizeElement = Math.Round((double)(sizeElement / 1024 / 1024 / 1024), 2);
                    }
                    else if (sizeElement >= 1024 * 1024)
                    {
                        typeOfSize = "MB";
                        sizeElement = Math.Round((double)(sizeElement / 1024 / 1024), 2);
                    }
                    else
                    {
                        typeOfSize = "KB";
                        sizeElement = Math.Round((double)(sizeElement / 1024), 2);
                    }

                    string name = item.Substring(++lastIndex);

                    files.Add(new FileModel
                    {
                        Name = name,
                        PathName = item,
                        SizeElement = sizeElement,
                        TypeOfSize = typeOfSize,
                        SizeElementToKb = Math.Round((double)(sizeElementToKb / 1024), 2),
                        Image = "/images/file.jpg",
                        NoAccessToFile = false
                    });
                }

                // инициализируем модель представления
                GetCatalogsAhdFilesViewModel model = new GetCatalogsAhdFilesViewModel
                {
                    Path = path,
                    Catalogs = catalogs,
                    Files = files
                };

                return model;
            }
            catch (UnauthorizedAccessException ex)
            {
                return null;
            }            
        }

        public static double SizeCatalog(string path, ref double sizeCatalog) // метод рекурсивного получения размера папок
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(path);
                DirectoryInfo[] diA = di.GetDirectories();
                FileInfo[] fiA = di.GetFiles();

                foreach (FileInfo item in fiA)
                {
                    sizeCatalog += item.Length;
                }

                foreach (DirectoryInfo item in diA)
                {
                    SizeCatalog(item.FullName, ref sizeCatalog);
                }

                return sizeCatalog;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        [HttpGet]
        public double GetSizeCatalogForView(string path) // for View
        {        
            double sizeCatalog = 0;
            sizeCatalog = SizeCatalog(path, ref sizeCatalog);

            // для более точного отслеживания нуля
            double testSizeCatalog = sizeCatalog;

            // отлавливаем каталоги без доступа(или с файлами без доступа)
            if (testSizeCatalog == 0)
            {
                try
                {
                    List<string> c = Directory.GetDirectories(path).ToList();
                    List<string> f = Directory.GetFiles(path).ToList();
                    if (c.Count() != 0 || f.Count() != 0)
                    {
                        testSizeCatalog = -1;
                    }
                }
                catch (Exception ex)
                {
                    testSizeCatalog = -1;
                }
                return testSizeCatalog;
            }         
            return sizeCatalog;
        }
    }
}
