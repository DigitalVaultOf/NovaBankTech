using Bank.Api.Model;
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
        public DbSet<Transfer> Transfers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Transfer>()
                .HasOne(t => t.AccountFrom)
                .WithMany(a => a.TransfersSent)
                .HasForeignKey(t => t.AccountNumberFrom)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Transfer>()
                .HasOne(t => t.AccountTo)
                .WithMany(a => a.TransfersReceived)
                .HasForeignKey(t => t.AccountNumberTo)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
