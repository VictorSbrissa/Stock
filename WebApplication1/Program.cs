using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using WebApplication1.Infrastructure;
using WebApplication1.Logic;
using WebApplication1.Logic.Tenancy;
using WebApplication1.Management;
using WebApplication1.Management.Models;

// --- CONFIGURAÇÃO INICIAL ---

// Desliga o mapeamento de claims padrão do JWT para usar os nomes originais (ex: "role")
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// 1. Serviços de Segurança (Autenticação e Autorização)
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<ManagementDbContext>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
builder.Services.AddAuthorization();

// 2. Serviços Padrão da API (Controllers, Swagger)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Serviços de Banco de Dados
var managementConnectionString = builder.Configuration.GetConnectionString("ManagementDbConnection");
builder.Services.AddDbContext<ManagementDbContext>(options =>
    options.UseMySql(managementConnectionString, ServerVersion.AutoDetect(managementConnectionString))
);

// 4. Serviços de Multi-Tenancy
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddDbContext<SystemContext>((serviceProvider, options) =>
{
    var tenantService = serviceProvider.GetRequiredService<ITenantService>();
    var tenantConnectionString = tenantService.GetConnectionString();
    options.UseMySql(tenantConnectionString, ServerVersion.AutoDetect(tenantConnectionString));
});

// 5. Serviços de Lógica de Negócio
builder.Services.AddScoped<CategoryLogic>();

// --- CONFIGURAÇÃO DO PIPELINE HTTP ---

var app = builder.Build();

// Seeding de Dados Iniciais (Roles)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await WebApplication1.Management.Seed.RolesSeeder.SeedRolesAsync(roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro durante o seeding das roles.");
    }
}

// Middlewares do Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();