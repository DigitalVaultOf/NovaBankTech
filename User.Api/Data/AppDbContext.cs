using Microsoft.EntityFrameworkCore;
using User.Api.Model;



namespace User.Api.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Users> Users { get; set; }

    }
}
