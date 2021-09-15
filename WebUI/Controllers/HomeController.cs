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
        public IActionResult GetPcDisk()
        {
            // получаем диски с ПК
            List<DriveInfo> pcDisks = DriveInfo.GetDrives().ToList();

            // инициализируем модели Disk
            List<Disk> disks = new List<Disk>();
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

                disks.Add(new Disk
                {
                    Name = item.Name.Trim('\\'),
                    NamePath = item.Name,
                    Space = Math.Round((double)item.TotalSize / 1024 / 1024 / 1024, 2),
                    FreeSpace = Math.Round((double)item.TotalFreeSpace / 1024 / 1024 / 1024, 2),
                    Image = image

                });
            }
            return View(disks);
        }

        [HttpGet]
        public IActionResult Path(string path) 
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
            List<Catalog> catalogs = new List<Catalog>();
            List<_File> files = new List<_File>();

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
                catalogs.Add(new Catalog
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

                sizeCatalog = Math.Round((double)(sizeCatalog / 1024 / 1024 / 1024), 2);

                // отлавливаем каталоги без доступа(или с файлами без доступа)
                if (testSizeCatalog==0)
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
                item.BusySpace = sizeCatalog;
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

                string name = item.Substring(++lastIndex);
                FileInfo toBusySpace = fileInfosForFiles.FirstOrDefault(f => f.FullName == item);
                files.Add(new _File
                {
                    Name = name,
                    PathName = item,
                    BusySpace = Math.Round((double)toBusySpace.Length / 1024 / 1024, 2),
                    Image = "/images/file.jpg",
                    NoAccessToFile = false
                });
            }
          
            // инициализируем модель представления
            PathViewModel model = new PathViewModel
            {
                Path = path,
                Catalogs = catalogs,
                Files = files
            };
                      
            return View(model);
        }

        [HttpPost]
        public IActionResult Path(string path, string formData, string filter)
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
            List<Catalog> catalogs = new List<Catalog>();
            List<_File> files = new List<_File>();

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
                catalogs.Add(new Catalog
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

                sizeCatalog = Math.Round((double)(sizeCatalog / 1024 / 1024 / 1024), 2);

                // отлавливаем каталоги без доступа(или с файлами без доступа)
                if (sizeCatalog == 0)
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
                item.BusySpace = sizeCatalog;
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

                string name = item.Substring(++lastIndex);
                FileInfo toBusySpace = fileInfosForFiles.FirstOrDefault(f => f.FullName == item);
                files.Add(new _File
                {
                    Name = name,
                    PathName = item,
                    BusySpace = Math.Round((double)toBusySpace.Length / 1024 / 1024, 2),
                    Image = "/images/file.jpg",
                    NoAccessToFile = false
                });
            }

            // инициализируем модель представления

            //фильтрация списка по размеру, по доступу
            if (filter!=null)
            {
                switch (filter)
                {
                    case "ascending":
                        List<Catalog> c = new List<Catalog>();                        
                        foreach (var item in catalogs)
                        {
                            if (item.NoAccessToFile==false)
                            {
                                c.Add(item);
                            }
                        }
                        List<_File> f = new List<_File>();
                        foreach (var item in files)
                        {
                            if (item.NoAccessToFile == false)
                            {
                                f.Add(item);
                            }
                        }
                        PathViewModel m = new PathViewModel
                        {
                            Path = path,
                            Catalogs = c.OrderBy(c=>c.BusySpace).ToList(),
                            Files = f.OrderBy(f => f.BusySpace).ToList(),
                        };
                        return View(m);

                    case "descending":
                        List<Catalog> c1 = new List<Catalog>();
                        foreach (var item in catalogs)
                        {
                            if (item.NoAccessToFile == false)
                            {
                                c1.Add(item);
                            }
                        }
                        List<_File> f1 = new List<_File>();
                        foreach (var item in files)
                        {
                            if (item.NoAccessToFile == false)
                            {
                                f1.Add(item);
                            }
                        }
                        PathViewModel m1 = new PathViewModel
                        {
                            Path = path,
                            Catalogs = c1.OrderByDescending(c => c.BusySpace).ToList(),
                            Files = f1.OrderByDescending(f => f.BusySpace).ToList(),
                        };
                        return View(m1);

                    case "noAccessToFile":
                        List<Catalog> c2 = new List<Catalog>();
                        foreach (var item in catalogs)
                        {
                            if (item.NoAccessToFile == true)
                            {
                                c2.Add(item);
                            }
                        }
                        List<_File> f2 = new List<_File>();
                        foreach (var item in files)
                        {
                            if (item.NoAccessToFile == true)
                            {
                                f2.Add(item);
                            }
                        }
                        PathViewModel m2 = new PathViewModel
                        {
                            Path = path,
                            Catalogs = c2.ToList(),
                            Files = f2.ToList(),
                        };
                        return View(m2);

                }
            }

            // для возвращения всего списка
            if (formData==null)
            {
                PathViewModel m = new PathViewModel
                {
                    Path = path,
                    Catalogs = catalogs,
                    Files = files
                };
                return View(m);
            }

            // фильтрация списка по имени
            PathViewModel model = new PathViewModel
            {
                Path = path,
                Catalogs = catalogs.Where(c => c.Name.Contains(formData)).ToList(),
                Files = files.Where(f => f.Name.Contains(formData)).ToList()
            };

            return View(model);
        }

        static double SizeCatalog(string path, ref double sizeCatalog ) 
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
    }
}
