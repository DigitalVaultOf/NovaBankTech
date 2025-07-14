using Microsoft.EntityFrameworkCore;
using Pix.Api.Models;

namespace Pix.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<PixModel> Pix { get; set; }
        public DbSet<TransactionModel> TransactionsTable { get; set; }

    }
}
