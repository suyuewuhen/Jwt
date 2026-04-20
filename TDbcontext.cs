using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Jwt
{
    public class TDbcontext:DbContext
    {
        public DbSet<DbJwtConfigProvider.SystemConfig> SystemConfigs { get; set; } = null!;

        public TDbcontext(DbContextOptions<TDbcontext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }
    }
}
