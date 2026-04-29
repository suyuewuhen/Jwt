using Microsoft.EntityFrameworkCore;

namespace Jwt.Implementations.Db;

/// <summary>
/// JWT配置数据库上下文
/// </summary>
public class JwtDbContext : DbContext
{
    public DbSet<SystemConfig> SystemConfigs { get; set; } = null!;

    public JwtDbContext(DbContextOptions<JwtDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<SystemConfig>()
            .HasKey(c => c.ConfigKey);
    }
}
