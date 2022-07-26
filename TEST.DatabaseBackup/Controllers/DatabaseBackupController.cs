using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using TEST.DatabaseBackup.Data;
using TEST.DatabaseBackup.Service;

namespace TEST.DatabaseBackup.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseBackupController : ControllerBase
	{
		private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostEnvironment _env;
        private readonly IBackupService _backupService;
		public DatabaseBackupController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IHostEnvironment env, IBackupService backupService)
		{
			_configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _env = env;
            _backupService = backupService;
        }

		[HttpGet("CreateBackup")]
		public IActionResult CreateBackup()
        {
            try
            {
                if (!_backupService.CreateDatabaseBackup(out var backupName)) return BadRequest();
                if (!_backupService.CreateZipFile(backupName, out var zipFileName)) return BadRequest();
                return Ok(zipFileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500);
            }
        }
	}
}
