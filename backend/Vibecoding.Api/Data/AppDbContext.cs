using Microsoft.EntityFrameworkCore;
using Vibecoding.Api.Models;

namespace Vibecoding.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TodoItem> Todos => Set<TodoItem>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(e =>
        {
            // 必须与迁移中的表名一致（否则运行时会去查 Users/users，而库里是 AppUsers）
            e.ToTable("AppUsers");
            e.HasIndex(x => x.UserName).IsUnique();
        });
    }
}