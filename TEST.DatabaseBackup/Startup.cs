using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TEST.DatabaseBackup.Data;
using TEST.DatabaseBackup.Service;

namespace TEST.DatabaseBackup
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            #region DB
            //NOTE: DataBase
            const string connectionString = "DatabaseConnection";
            services.AddDbContextPool<ApplicationContext>(
                options =>
                    options.UseSqlServer(
                        Configuration.GetConnectionString(connectionString), sqloptions =>
                        {
                            sqloptions.EnableRetryOnFailure(
                                maxRetryCount: 3,
                                maxRetryDelay: TimeSpan.FromSeconds(10),
                                errorNumbersToAdd: new List<int>() { });
                        })
            );
            #endregion

            #region DI
            //NOTE: Dependency Injection
            services.AddScoped<IBackupService, BackupService>();
            services.AddScoped<IFileManager, FileManager>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            #endregion

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TEST.DatabaseBackupController", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TEST.DatabaseBackupController v1"));
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
