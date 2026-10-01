using Microsoft.EntityFrameworkCore;
using PaginationLab.Api.Models;

namespace PaginationLab.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(order => order.Id);
            entity.Property(order => order.Number)
                .HasMaxLength(20)
                .IsRequired();
        });
    }
}
