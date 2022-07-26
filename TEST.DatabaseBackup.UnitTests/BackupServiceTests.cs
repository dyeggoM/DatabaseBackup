using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using TEST.DatabaseBackup.Controllers;
using TEST.DatabaseBackup.Data;
using TEST.DatabaseBackup.Service;

namespace TEST.DatabaseBackup.UnitTests
{
    [TestFixture]
    class BackupServiceTests
    {
        #region SetUp
        private BackupService _backupService;
        private Mock<IHostEnvironment> _mockEnvironment;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ApplicationContext> _mockContext;
        private Mock<IFileManager> _mockFileManager;
        private string environmentPath = "environment";
        private string backupFolder = "BackupFolder";


        [SetUp]
        public void SetUp()
        {
            _mockEnvironment = new Mock<IHostEnvironment>();
            _mockEnvironment
                .Setup(environment => environment.ContentRootPath)
                .Returns(environmentPath);

            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration
                .Setup(configuration => configuration[It.Is<string>(s=>s == "DatabaseBackup:BackupFolder")])
                .Returns(backupFolder);

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            _mockContext = new Mock<ApplicationContext>(optionsBuilder.Options);

            _mockFileManager = new Mock<IFileManager>();
            _mockFileManager.Setup(fileManager => fileManager.CreateDirectory(It.IsAny<string>()));

            _backupService = new BackupService(
                _mockContext.Object, 
                _mockConfiguration.Object, 
                _mockEnvironment.Object,
                _mockFileManager.Object);
        }

        private string BackupNameAndPathSetUp(out string pathToSave)
        {
            var time = DateTime.UtcNow.AddHours(-5);
            var expectedName = $"database_backup_{time:yyyyMMdd}_{time:HHmm}";

            pathToSave = Path.Combine(environmentPath, backupFolder);

            return expectedName;
        }
        #endregion

        [Test]
        public void CreateDatabaseBackup_WhenCalled_ReturnsTrue()
        {
            var expectedName = BackupNameAndPathSetUp(out var pathToSave);
            var backupFileName = $"{expectedName}.Bak";
            
            _mockContext
                .Setup(context => context.CreateBackup(It.IsAny<string>()))
                .Returns(true);

            var result = _backupService.CreateDatabaseBackup(out var resultName);

            Assert.Multiple(() =>
            {
                _mockFileManager.Verify(fileManager=>fileManager.CreateDirectory(pathToSave),Times.Once);
                var backupPath = Path.Combine(pathToSave, backupFileName);
                _mockContext.Verify(context => context.CreateBackup(backupPath),
                    Times.Once);
                Assert.IsTrue(result);
                Assert.That(resultName, Is.EqualTo(expectedName));
            });
        }

        [Test]
        public void CreateDatabaseBackup_WhenCreateBackupThrowsException_ReturnsFalse()
        {
            var expectedName = BackupNameAndPathSetUp(out var pathToSave);
            var backupFileName = $"{expectedName}.Bak";

            _mockContext
                .Setup(context => context.CreateBackup(It.IsAny<string>()))
                .Throws<Exception>();

            var result = _backupService.CreateDatabaseBackup(out var resultName);

            Assert.Multiple(() =>
            {
                _mockFileManager.Verify(fileManager => fileManager.CreateDirectory(pathToSave), Times.Once);
                var backupPath = Path.Combine(pathToSave, backupFileName);
                _mockContext.Verify(context => context.CreateBackup(backupPath),
                    Times.Once);
                Assert.IsFalse(result);
                Assert.That(resultName, Is.EqualTo(expectedName));
            });
        }

        [TestCase(true,true,1,1,true)]
        [TestCase(false,true,1,0,false)]
        [TestCase(true, false, 1, 1,false)]
        public void CreateZipFile_WhenCalled_ReturnsTrueOrFalse(
            bool mockCreateZipFileResult, 
            bool mockDeleteBackupFileResult,
            int createZipFileTimesCalledResult,
            int deleteBackupFileTimesCalledResult,
            bool expectedResult
            )
        {
            var backupName = BackupNameAndPathSetUp(out var pathToSave);
            var expectedName = $"{backupName}.zip";

            _mockFileManager
                .Setup(fileManager => fileManager.CreateZipFile(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockCreateZipFileResult);
            _mockFileManager
                .Setup(fileManager => fileManager.DeleteBackupFile(It.IsAny<string>()))
                .Returns(mockDeleteBackupFileResult);

            var result = _backupService.CreateZipFile(backupName, out var resultName);

            Assert.Multiple(() =>
            {
                var zipFilePath = Path.Combine(pathToSave, expectedName);
                var backupFilePath = Path.Combine(pathToSave, $"{backupName}.Bak");
                _mockFileManager.Verify(fileManager => fileManager.CreateZipFile(zipFilePath, backupFilePath), Times.Exactly(createZipFileTimesCalledResult));
                _mockFileManager.Verify(fileManager => fileManager.DeleteBackupFile(backupFilePath), Times.Exactly(deleteBackupFileTimesCalledResult));
                Assert.That(result,Is.EqualTo(expectedResult));
                Assert.That(resultName, Is.EqualTo(expectedName));
            });
        }

        [Test]
        public void CreateZipFile_WhenCreateZipFileThrowsException_ReturnsFalse()
        {
            var backupName = BackupNameAndPathSetUp(out var pathToSave);
            var expectedName = $"{backupName}.zip";

            _mockFileManager
                .Setup(fileManager => fileManager.CreateZipFile(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<Exception>();
            _mockFileManager
                .Setup(fileManager => fileManager.DeleteBackupFile(It.IsAny<string>()))
                .Returns(true);

            var result = _backupService.CreateZipFile(backupName, out var resultName);

            Assert.Multiple(() =>
            {
                var zipFilePath = Path.Combine(pathToSave, expectedName);
                var backupFilePath = Path.Combine(pathToSave, $"{backupName}.Bak");
                _mockFileManager.Verify(fileManager => fileManager.CreateZipFile(zipFilePath, backupFilePath), Times.Once);
                _mockFileManager.Verify(fileManager => fileManager.DeleteBackupFile(backupFilePath), Times.Never);
                Assert.IsFalse(result);
                Assert.That(resultName, Is.EqualTo(expectedName));
            });
        }

        [Test]
        public void CreateZipFile_WhenDeleteBackupFileThrowsException_ReturnsFalse()
        {
            var backupName = BackupNameAndPathSetUp(out var pathToSave);
            var expectedName = $"{backupName}.zip";

            _mockFileManager
                .Setup(fileManager => fileManager.CreateZipFile(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
            _mockFileManager
                .Setup(fileManager => fileManager.DeleteBackupFile(It.IsAny<string>()))
                .Throws<Exception>();

            var result = _backupService.CreateZipFile(backupName, out var resultName);

            Assert.Multiple(() =>
            {
                var zipFilePath = Path.Combine(pathToSave, expectedName);
                var backupFilePath = Path.Combine(pathToSave, $"{backupName}.Bak");
                _mockFileManager.Verify(fileManager => fileManager.CreateZipFile(zipFilePath, backupFilePath), Times.Once);
                _mockFileManager.Verify(fileManager => fileManager.DeleteBackupFile(backupFilePath), Times.Once);
                Assert.IsFalse(result);
                Assert.That(resultName, Is.EqualTo(expectedName));
            });
        }

    }
}
