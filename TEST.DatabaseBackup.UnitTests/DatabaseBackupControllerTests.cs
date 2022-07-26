using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TEST.DatabaseBackup.Controllers;
using TEST.DatabaseBackup.Data;
using TEST.DatabaseBackup.Service;

namespace TEST.DatabaseBackup.UnitTests
{
    [TestFixture]
    class DatabaseBackupControllerTests
    {
        #region SetUp
        private DatabaseBackupController _controller;
        private Mock<IHostEnvironment> _mockEnv;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private Mock<IBackupService> _mockBackupService;
        private string backupFileName;
        private string zipFileName;

        [SetUp]
        public void SetUp()
        {
            _mockEnv = new Mock<IHostEnvironment>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockBackupService = new Mock<IBackupService>();
            _controller = new DatabaseBackupController(
                _mockConfiguration.Object,
                _mockHttpContextAccessor.Object,
                _mockEnv.Object, 
                _mockBackupService.Object);
        }
        #endregion

        [Test]
        public void CreateBackup_WhenCalled_ReturnsOkWithZipFileName()
        {
            _mockBackupService
                .Setup(backupService => backupService.CreateDatabaseBackup(out backupFileName))
                .Returns(true);
            _mockBackupService
                .Setup(backupService => backupService.CreateZipFile(It.IsAny<string>(),out zipFileName))
                .Returns(true);
            var result = _controller.CreateBackup();
            Assert.Multiple(() =>
            {
                _mockBackupService.Verify(backupService => backupService.CreateDatabaseBackup(out backupFileName), 
                    Times.Once);
                _mockBackupService.Verify(backupService => backupService.CreateZipFile(backupFileName, out zipFileName), 
                    Times.Once);
                Assert.That(result, Is.TypeOf<OkObjectResult>());
            });
        }

        [TestCase(false, true, 1, 0)]
        [TestCase(true, false, 1,1)]
        public void CreateBackup_WhenCreateDatabaseBackupOrCreateZipFileReturnsFalse_ReturnsBadRequest(
            bool mockDatabaseBackupResult,
            bool mockZipFileResult,
            int databaseBackupTimesCalledResult,
            int zipFileTimesCalledResult
            )
        {
            _mockBackupService
                .Setup(backupService => backupService.CreateDatabaseBackup(out backupFileName))
                .Returns(mockDatabaseBackupResult);
            _mockBackupService
                .Setup(backupService => backupService.CreateZipFile(It.IsAny<string>(), out zipFileName))
                .Returns(mockZipFileResult);
            var result = _controller.CreateBackup();
            Assert.Multiple(() =>
            {
                _mockBackupService.Verify(backupService => backupService.CreateDatabaseBackup(out backupFileName), 
                    Times.Exactly(databaseBackupTimesCalledResult));
                _mockBackupService.Verify(backupService => backupService.CreateZipFile(backupFileName, out zipFileName), 
                    Times.Exactly(zipFileTimesCalledResult));
                Assert.That(result, Is.TypeOf<BadRequestResult>());
            });
        }

        [Test]
        public void CreateBackup_WhenExceptionThrownAtCreateDatabaseBackup_ReturnsStatusCode500()
        {
            _mockBackupService
                .Setup(backupService => backupService.CreateDatabaseBackup(out backupFileName))
                .Throws<Exception>();
            _mockBackupService
                .Setup(backupService => backupService.CreateZipFile(It.IsAny<string>(), out zipFileName))
                .Throws<Exception>();
            var result = _controller.CreateBackup();
            Assert.Multiple(() =>
            {
                _mockBackupService.Verify(backupService => backupService.CreateDatabaseBackup(out backupFileName),
                    Times.Once);
                _mockBackupService.Verify(backupService => backupService.CreateZipFile(backupFileName, out zipFileName),
                    Times.Never);
                Assert.Throws<Exception>(() => _mockBackupService.Object.CreateDatabaseBackup(out backupFileName));
                Assert.That((result as StatusCodeResult)?.StatusCode, Is.EqualTo(500));
            });
        }
        [Test]
        public void CreateBackup_WhenExceptionThrownAtCreateZipFile_ReturnsStatusCode500()
        {
            _mockBackupService
                .Setup(backupService => backupService.CreateDatabaseBackup(out backupFileName))
                .Returns(true);
            _mockBackupService
                .Setup(backupService => backupService.CreateZipFile(It.IsAny<string>(), out zipFileName))
                .Throws<Exception>();
            var result = _controller.CreateBackup();
            Assert.Multiple(() =>
            {
                _mockBackupService.Verify(backupService => backupService.CreateDatabaseBackup(out backupFileName),
                    Times.Once);
                _mockBackupService.Verify(backupService => backupService.CreateZipFile(backupFileName, out zipFileName),
                    Times.Once);
                Assert.Throws<Exception>(() => _mockBackupService.Object.CreateZipFile(backupFileName, out backupFileName));
                Assert.That((result as StatusCodeResult)?.StatusCode, Is.EqualTo(500));
            });
        }
    }
}
