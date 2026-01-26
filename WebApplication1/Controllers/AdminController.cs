// Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Infrastructure; // Onde está seu SystemContext
using WebApplication1.Management;    // Onde está seu ManagementDbContext

[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = "Admin")] // <-- Adicione segurança aqui mais tarde!
public class AdminController : ControllerBase
{
    private readonly ManagementDbContext _managementDb;
    private readonly IServiceProvider _serviceProvider;

    public AdminController(ManagementDbContext managementDb, IServiceProvider serviceProvider)
    {
        _managementDb = managementDb;
        _serviceProvider = serviceProvider;
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
                using (var tenantDbContext = new SystemContext(dbContextOptions))
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
}