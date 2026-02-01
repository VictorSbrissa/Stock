using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SeuProjeto.Models;
using System.IO;
using WebApplication1.Logic.Tenancy; // <-- Adicione o using para o ITenantService
using WebApplication1.Models; // <-- Adicione o using para a Category

namespace WebApplication1.Infrastructure
{
    // 1. CORREÇÃO DO SYSTEMCONTEXT
    public class SystemContext : DbContext
    {
        // Deixe apenas este construtor. Ele será usado tanto pelo runtime quanto pela factory.
        public SystemContext(DbContextOptions<SystemContext> options, ITenantService tenantService)
            : base(options)
        {
            // O tenantService é injetado aqui.
            // Em tempo de execução, ele terá um valor.
            // Em tempo de design (via factory), ele será null, mas não tem problema.
        }

        public DbSet<Category> Category { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Venda> Vendas { get; set; }
    }

    // 2. FACTORY CORRIGIDA (sem código duplicado)
    public class SystemContextFactory : IDesignTimeDbContextFactory<SystemContext>
    {
        public SystemContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            connectionString = connectionString.Replace("__TENANT_DB__", "design_time_db");

            var optionsBuilder = new DbContextOptionsBuilder<SystemContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            // A ferramenta 'dotnet ef' é inteligente. Ela sabe como criar o SystemContext
            // mesmo que o construtor peça por um ITenantService. Ela simplesmente passará 'null'
            // para esse parâmetro, o que é suficiente para criar a migração.
            return new SystemContext(optionsBuilder.Options, null); // Passando null para o tenantService
        }
    }
}
