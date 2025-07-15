using Microsoft.EntityFrameworkCore;

namespace Pix.Api.Data.Scripts
{
    public static class SqlScriptExecutor
    {
        public static void ExecuteSqlScriptsFromFolder(DbContext dbContext, string folderPath)
        {
            var connection = dbContext.Database.GetDbConnection();
            connection.Open();

            foreach (var file in Directory.GetFiles(folderPath, "*.sql"))
            {
                var sql = File.ReadAllText(file);

                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }

            connection.Close();
        }
    }
}
