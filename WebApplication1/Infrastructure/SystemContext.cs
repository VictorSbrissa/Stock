﻿using WebApplication1.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;


namespace WebApplication1.Infrastructure
{
    public class SystemContext : DbContext
    {
        public SystemContext(DbContextOptions<SystemContext> options) : base(options) { }

        public DbSet<Category> Category { get; set; }

    }

    public class DbContextFactory : IDesignTimeDbContextFactory<SystemContext>
    {
        public SystemContext CreateDbContext(string[] args)
        {
            // Configuração para ler o appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Pega o diretório atual do projeto
                .AddJsonFile("appsettings.json") // Adiciona o appsettings.json
                .Build();

            // Pega a string de conexão da seção "ConnectionStrings"
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<SystemContext>();
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 40)); // Ou a sua versão específica

            optionsBuilder.UseMySql(connectionString, serverVersion);

            return new SystemContext(optionsBuilder.Options);
        }
    }
}