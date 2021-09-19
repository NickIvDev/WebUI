using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        [HttpPost]
        public IActionResult GetSortDataBySize(string path, string sortBySize) // сортировка переданного каталога по размеру
        {
            GetCatalogsAhdFilesViewModel tempModel = GetDataViewModel(path);

            if (sortBySize != null)
            {
                switch (sortBySize)
                {
                    case "ascending":
                        List<CatalogModel> c = new List<CatalogModel>();
                        foreach (var item in tempModel.Catalogs)
                        {
                            if (item.NoAccessToFile == false)
                            {
                                c.Add(item);
                            }
                        }
                        List<FileModel> f = new List<FileModel>();
                        foreach (var item in tempModel.Files)
                        {
                            if (item.NoAccessToFile == false)
                            {
                                f.Add(item);
                            }
                        }
                        GetCatalogsAhdFilesViewModel m = new GetCatalogsAhdFilesViewModel
                        {
                            Path = path,
                            Catalogs = c.OrderBy(c => c.SizeElementToKb).ToList(),
                            Files = f.OrderBy(f => f.SizeElementToKb).ToList(),
                        };
                        return View("GetCatalogsAhdFiles", m);

                    case "descending":
                        List<CatalogModel> c1 = new List<CatalogModel>();
                        foreach (var item in tempModel.Catalogs)
                        {
                            if (item.NoAccessToFile == false)
                            {
                                c1.Add(item);
                            }
                        }
                        List<FileModel> f1 = new List<FileModel>();
                        foreach (var item in tempModel.Files)
                        {
                            if (item.NoAccessToFile == false)
                            {
                                f1.Add(item);
                            }
                        }
                        GetCatalogsAhdFilesViewModel m1 = new GetCatalogsAhdFilesViewModel
                        {
                            Path = path,
                            Catalogs = c1.OrderByDescending(c => c.SizeElementToKb).ToList(),
                            Files = f1.OrderByDescending(f => f.SizeElementToKb).ToList(),
                        };
                        return View("GetCatalogsAhdFiles", m1);

                    case "noAccessToFile":
                        List<CatalogModel> c2 = new List<CatalogModel>();
                        foreach (var item in tempModel.Catalogs)
                        {
                            if (item.NoAccessToFile == true)
                            {
                                c2.Add(item);
                            }
                        }
                        List<FileModel> f2 = new List<FileModel>();
                        foreach (var item in tempModel.Files)
                        {
                            if (item.NoAccessToFile == true)
                            {
                                f2.Add(item);
                            }
                        }
                        GetCatalogsAhdFilesViewModel m2 = new GetCatalogsAhdFilesViewModel
                        {
                            Path = path,
                            Catalogs = c2.ToList(),
                            Files = f2.ToList(),
                        };
                        return View("GetCatalogsAhdFiles", m2);
                }
            }
            return default;
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
                        PathName = item,
                        Image = "/images/folder.jpg",
                        NoAccessToFile = false
                    });
                }

                // рекурсия - получение размера каталогов
                foreach (var item in catalogs)
                {
                    double sizeCatalog = 0;
                    sizeCatalog = SizeCatalog(item.PathName, ref sizeCatalog);

                    // для более точного отслеживания нуля
                    double testSizeCatalog = sizeCatalog;

                    // отлавливаем каталоги без доступа(или с файлами без доступа)
                    if (testSizeCatalog == 0)
                    {
                        try
                        {
                            List<string> c = Directory.GetDirectories(item.PathName).ToList();
                            List<string> f = Directory.GetFiles(item.PathName).ToList();
                            if (c.Count() != 0 || f.Count() != 0)
                            {
                                item.NoAccessToFile = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            item.NoAccessToFile = true;
                        }
                    }

                    if (testSizeCatalog >= 1024 * 1024 * 1024)
                    {
                        item.TypeOfSize = "GB";
                        item.SizeElement = Math.Round((double)(sizeCatalog / 1024 / 1024 / 1024), 2);
                        item.SizeElementToKb = Math.Round((double)(sizeCatalog / 1024), 2);
                    }
                    else if (testSizeCatalog >= 1024 * 1024)
                    {
                        item.TypeOfSize = "MB";
                        item.SizeElement = Math.Round((double)(sizeCatalog / 1024 / 1024), 2);
                        item.SizeElementToKb = Math.Round((double)(sizeCatalog / 1024), 2);
                    }
                    else
                    {
                        item.TypeOfSize = "KB";
                        item.SizeElement = Math.Round((double)(sizeCatalog / 1024), 2);
                        item.SizeElementToKb = Math.Round((double)(sizeCatalog / 1024), 2);
                    }
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

        //public CatalogModel GetSizeCatalogForView(CatalogModel catalog) // for View
        //{
        //    double sizeCatalog = SizeCatalog2(catalog.PathName);

        //    // для более точного отслеживания нуля
        //    double testSizeCatalog = sizeCatalog;

        //    // отлавливаем каталоги без доступа(или с файлами без доступа)
        //    if (testSizeCatalog == 0)
        //    {
        //        try
        //        {
        //            List<string> c = Directory.GetDirectories(catalog.PathName).ToList();
        //            List<string> f = Directory.GetFiles(catalog.PathName).ToList();
        //            if (c.Count() != 0 || f.Count() != 0)
        //            {
        //                catalog.NoAccessToFile = true;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            catalog.NoAccessToFile = true;
        //        }
        //    }

        //    if (testSizeCatalog >= 1024 * 1024 * 1024)
        //    {
        //        catalog.TypeOfSize = "GB";
        //        catalog.SizeElement = Math.Round((double)(sizeCatalog / 1024 / 1024 / 1024), 2);
        //        catalog.SizeElementToKb = Math.Round((double)(sizeCatalog / 1024), 2);
        //    }
        //    else if (testSizeCatalog >= 1024 * 1024)
        //    {
        //        catalog.TypeOfSize = "MB";
        //        catalog.SizeElement = Math.Round((double)(sizeCatalog / 1024 / 1024), 2);
        //        catalog.SizeElementToKb = Math.Round((double)(sizeCatalog / 1024), 2);
        //    }
        //    else
        //    {
        //        catalog.TypeOfSize = "KB";
        //        catalog.SizeElement = Math.Round((double)(sizeCatalog / 1024), 2);
        //        catalog.SizeElementToKb = Math.Round((double)(sizeCatalog / 1024), 2);
        //    }
        //    return catalog;
        //}

        //public static double SizeCatalog2(string path) // метод рекурсивного получения размера папок for View
        //{
        //    try
        //    {
        //        double sizeCatalog = 0;
        //        DirectoryInfo di = new DirectoryInfo(path);
        //        FileInfo[] fiA = di.GetFiles();
        //        foreach (FileInfo fi in fiA)
        //        {
        //            sizeCatalog += fi.Length;
        //        }

        //        DirectoryInfo[] diA = di.GetDirectories();
        //        foreach (DirectoryInfo d in diA)
        //        {
        //            sizeCatalog += SizeCatalog2(d.FullName);
        //        }

        //        return sizeCatalog;
        //    }
        //    catch (Exception ex)
        //    {
        //        return 0;
        //    }
        //}
    }
}
