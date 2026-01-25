using WebApplication1.Management;

namespace WebApplication1.Logic.Tenancy
{
    public interface ITenantService
    {
        string GetConnectionString();
        string GetTenantId();
    }
    public class TenantService : ITenantService
    {
        private readonly HttpContext _httpContext;
        private readonly ManagementDbContext _managementDb;
        private TenantInfo _tenant;

        public TenantService(IHttpContextAccessor httpContextAccessor, ManagementDbContext managementDb)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _managementDb = managementDb;
        }

        public string GetTenantId()
        {
            // Identifica o tenant pelo cabeçalho "X-Tenant-ID"
            return _httpContext.Request.Headers["X-Tenant-ID"].FirstOrDefault();
        }

        public string GetConnectionString()
        {
            if (_tenant != null) return _tenant.ConnectionString;

            var tenantId = GetTenantId();
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new InvalidOperationException("Tenant ID not found in request headers.");
            }

            // Busca as informações do tenant no banco de gerenciamento
            _tenant = _managementDb.Tenants.FirstOrDefault(t => t.TenantId == tenantId);
            if (_tenant == null)
            {
                throw new InvalidOperationException($"Tenant '{tenantId}' not found.");
            }

            return _tenant.ConnectionString;
        }
    }
}
