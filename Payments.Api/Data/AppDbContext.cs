using Microsoft.EntityFrameworkCore;
using Payments.Api.Models;

namespace Payments.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MonthPaymentModel> MonthPayments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<MonthPaymentModel>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
        });
    }
}