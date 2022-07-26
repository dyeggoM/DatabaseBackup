using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace TEST.DatabaseBackup.Service
{
    public interface IFileManager
    {
        void CreateDirectory(string path);
        bool CreateZipFile(string zipFilePath, string backupFilePath);
        bool DeleteBackupFile(string backupFilePath);
    }

    public class FileManager : IFileManager
    {

        public void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public bool CreateZipFile(string zipFilePath, string backupFilePath)
        {
            try
            {
                var backupFile = new FileInfo(backupFilePath);

                using (var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(backupFile.FullName, backupFile.Name);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public bool DeleteBackupFile(string backupFilePath)
        {
            try
            {
                var backupFile = new FileInfo(backupFilePath);

                backupFile.Delete();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}
