using Microsoft.EntityFrameworkCore;
using WebApplication1.Management;

namespace WebApplication1.Management
{
    public class ManagementDbContext : DbContext
    {
        public ManagementDbContext(DbContextOptions<ManagementDbContext> options) : base(options) { }

        public DbSet<TenantInfo> Tenants { get; set; }
    }
}


