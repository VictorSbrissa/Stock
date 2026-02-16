// Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApplication1.Infrastructure; 
using WebApplication1.Management;
using WebApplication1.Models;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ManagementDbContext _managementDb;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public AdminController(ManagementDbContext managementDb, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _managementDb = managementDb;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    [HttpPost("migrate-all-tenants")]
    public async Task<IActionResult> MigrateAllTenants()
    {
        // 1. Obter a lista de todos os tenants do banco de gerenciamento
        var allTenants = await _managementDb.Tenants.ToListAsync();
        if (allTenants == null || !allTenants.Any())
        {
            return NotFound("Nenhum tenant encontrado para migrar.");
        }

        var migrationResults = new Dictionary<string, string>();

        // 2. Iterar por cada tenant
        foreach (var tenant in allTenants)
        {
            try
            {
                // 3. Criar um DbContextOptions dinâmico para este tenant específico
                var dbContextOptions = new DbContextOptionsBuilder<SystemContext>()
                    .UseMySql(tenant.ConnectionString, ServerVersion.AutoDetect(tenant.ConnectionString))
                    .Options;

                // 4. Criar uma instância do DbContext do tenant com essas opções
                using (var tenantDbContext = new SystemContext(dbContextOptions, null))
                {
                    // 5. Executar a migração programaticamente
                    await tenantDbContext.Database.MigrateAsync();
                    migrationResults[tenant.TenantId] = "Sucesso";
                }
            }
            catch (Exception ex)
            {
                // Se a migração falhar para um tenant, registre o erro e continue para o próximo
                migrationResults[tenant.TenantId] = $"Falha: {ex.Message}";
            }
        }

        // Retorna um relatório de quais tenants foram migrados com sucesso ou falha
        return Ok(migrationResults);
    }

    [HttpPost("provision-tenant")]
    public async Task<IActionResult> ProvisionTenant([FromBody] CreateTenantRequest request)
    {
        // 1. Validar se o Tenant ID já existe
        var tenantId = request.TenantId.ToLower();
        var tenantExists = await _managementDb.Tenants.AnyAsync(t => t.TenantId == tenantId);
        if (tenantExists)
        {
            return Conflict($"O Tenant com o ID '{tenantId}' já existe.");
        }

        // 2. Construir a string de conexão para o novo tenant
        // Usaremos um template para padronizar o nome do banco de dados.
        var newDbName = $"tenant_{tenantId}";
        // ATENÇÃO: Pegue um modelo de string de conexão do appsettings.json para não repetir as credenciais.
        var connectionStringTemplate = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionStringTemplate))
        {
            return StatusCode(500, "O template da string de conexão 'DefaultConnection' não foi encontrado no appsettings.json.");
        }
        var newConnectionString = connectionStringTemplate.Replace("__TENANT_DB__", newDbName);

        // 3. Criar o banco de dados e aplicar as migrações
        try
        {
            var dbContextOptions = new DbContextOptionsBuilder<SystemContext>()
                .UseMySql(newConnectionString, ServerVersion.AutoDetect(newConnectionString))
                .Options;

            using (var tenantDbContext = new SystemContext(dbContextOptions, null))
            {
                // Database.MigrateAsync() é inteligente:
                // - Se o banco de dados não existir, ele o cria.
                // - Em seguida, aplica TODAS as migrações existentes.
                await tenantDbContext.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            // Se a criação ou migração do banco falhar, retorne um erro e não salve o tenant.
            return StatusCode(500, $"Falha ao criar ou migrar o banco de dados para o tenant '{tenantId}'. Erro: {ex.Message}");
        }

        // 4. Registrar o novo tenant no banco de dados de gerenciamento
        var newTenantInfo = new TenantInfo
        {
            TenantId = tenantId,
            ConnectionString = newConnectionString
        };

        _managementDb.Tenants.Add(newTenantInfo);
        await _managementDb.SaveChangesAsync();

        return Ok(new { Message = $"Tenant '{tenantId}' provisionado com sucesso.", ConnectionString = newConnectionString });
    }
}