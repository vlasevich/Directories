using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using FileDirBrowserServer.Models;

namespace FileDirBrowserServer.Controllers
{
    public class DefaultController : ApiController
    {
        private readonly int[] difSizeArray = new int[3];
        private static int mega = 1024;

        [HttpGet]
        public ExplorerModel Index(string realpath = "")
        {
            ExplorerModel explorerModel;
            if (string.IsNullOrEmpty(realpath))
            {
                IEnumerable<string> drivesList = Environment.GetLogicalDrives();
                List<DirModel> drivesListModel = drivesList.Select(drive => new DirModel {
                    DirName = drive
                }).ToList();

                explorerModel = new ExplorerModel(drivesListModel, null, realpath);
            }
            else
            {
                IEnumerable<string> dirList = Directory.EnumerateDirectories(realpath);

                List<DirModel> dirListModel = (from dir in dirList let d = new DirectoryInfo(dir) select new DirModel {
                    DirName = Path.GetFileName(dir) }
                ).ToList();

                IEnumerable<string> fileList = Directory.EnumerateFiles(realpath);
                List<FileModel> fileListModel =
                                                (from file in fileList
                                                 let f = new FileInfo(file)
                                                 select new FileModel
                                                 {
                                                     Name = Path.GetFileName(file),
                                                     FileSize = (f.Length < mega) ? f.Length : (double)f.Length / mega
                                                 }).ToList();

                explorerModel = new ExplorerModel(dirListModel, fileListModel, realpath);
            }
            
            return explorerModel;
        }

        [HttpGet]
        public int[] GetDirectoryFilesSizeStatistic(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            try
            {
                FileInfo[] files = directory.GetFiles();
                foreach (FileInfo file in files)
                {
                    double size = (double) file.Length/1048576;
                    if (size < 10)
                    {
                        difSizeArray[0] += 1;
                    }
                    else if (size >= 10 && size < 50)
                    {
                        difSizeArray[1] += 1;
                    }
                    else
                    {
                        difSizeArray[2] += 1;
                    }
                }

                DirectoryInfo[] dirs = directory.GetDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    GetDirectoryFilesSizeStatistic(dir.FullName);
                }
            }
            catch (UnauthorizedAccessException){}
            catch (DirectoryNotFoundException){}
            catch (StackOverflowException){}

            return difSizeArray;
        }
    }
}
