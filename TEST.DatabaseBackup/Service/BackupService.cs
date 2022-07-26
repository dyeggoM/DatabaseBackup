using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.IO.Compression;
using System.Security.AccessControl;
using TEST.DatabaseBackup.Data;

namespace TEST.DatabaseBackup.Service
{
    public interface IBackupService
    {
        bool CreateDatabaseBackup(out string backupFileName);
        bool CreateZipFile(string fileName, out string zipFileName);

    }
    public class BackupService : IBackupService
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly IFileManager _fileManager;

        public BackupService(ApplicationContext context, IConfiguration configuration, IHostEnvironment environment, IFileManager fileManager)
        {
            _context = context;
            _configuration = configuration;
            _environment = environment;
            _fileManager = fileManager;
        }

        public bool CreateDatabaseBackup(out string backupName)
        {
            var time = DateTime.UtcNow.AddHours(-5);
            backupName = $"database_backup_{time:yyyyMMdd}_{time:HHmm}";
            var backupFileName = $"{backupName}.Bak";

            try
            {
                var pathToSave = Path.Combine(_environment.ContentRootPath, _configuration["DatabaseBackup:BackupFolder"]);

                _fileManager.CreateDirectory(pathToSave);

                return _context.CreateBackup(Path.Combine(pathToSave, backupFileName));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }


        public bool CreateZipFile(string backupName, out string zipFileName)
        {
            zipFileName = $"{backupName}.zip";

            try
            {
                var pathToSave = Path.Combine(_environment.ContentRootPath, _configuration["DatabaseBackup:BackupFolder"]);
                var zipFilePath = Path.Combine(pathToSave, zipFileName);
                var backupFilePath = Path.Combine(pathToSave, $"{backupName}.Bak");

                return _fileManager.CreateZipFile(zipFilePath, backupFilePath) && _fileManager.DeleteBackupFile(backupFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}
