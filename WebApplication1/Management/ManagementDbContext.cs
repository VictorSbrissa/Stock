using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WebApplication1.Management;
using Microsoft.Extensions.Configuration;
using System.IO;
using WebApplication1.Management.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebApplication1.Management
{
    public class ManagementDbContext : IdentityDbContext<AppUser>
    {
        public ManagementDbContext(DbContextOptions<ManagementDbContext> options) : base(options) { }

        // O DbSet<TenantInfo> será gerenciado aqui
        public DbSet<TenantInfo> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // ESSENCIAL: Isso configura as tabelas do Identity

            // Se você tiver configurações personalizadas para suas tabelas (como TenantInfo),
            // elas podem vir aqui.
        }
    }
    //public class ManagementDbContext : DbContext
    //{
    //    public ManagementDbContext(DbContextOptions<ManagementDbContext> options) : base(options) { }

    //    public DbSet<TenantInfo> Tenants { get; set; }
    //}

    public class ManagementDbContextFactory : IDesignTimeDbContextFactory<ManagementDbContext>
    {
        public ManagementDbContext CreateDbContext(string[] args)
        {
            // Lê o appsettings.json para encontrar a string de conexão do banco de gerenciamento
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("ManagementDbConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ManagementDbContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new ManagementDbContext(optionsBuilder.Options);
        }
    }
}


