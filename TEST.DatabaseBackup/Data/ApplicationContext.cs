using System;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TEST.DatabaseBackup.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public virtual bool CreateBackup(string fileFullPath)
        {
            try
            {
                #region Create Backup file
                //NOTE: Creates backup from database to the specified path.
                SqlCommand sqlCommand = new SqlCommand(
                    "BACKUP DATABASE @db TO DISK = @fn;",
                    new SqlConnection(Database.GetDbConnection().ConnectionString));
                sqlCommand.CommandTimeout = 360;
                sqlCommand.Connection.Open();
                sqlCommand.Parameters.AddWithValue("@db", Database.GetDbConnection().Database);
                sqlCommand.Parameters.AddWithValue("@fn", fileFullPath);
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Connection.Close();
                #endregion
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
