using WebApplication1.Infrastructure;
using WebApplication1.Logic;
using WebApplication1.Middlewares;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using WebApplication1.Management;
using WebApplication1.Logic.Tenancy;
using WebApplication1.Infrastructure;
using WebApplication1.Logic;
using WebApplication1.Management; // Contexto de gerenciamento
using WebApplication1.Logic.Tenancy; // Serviços de tenant
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApplication1.Management.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);


// --- INÍCIO DA CONFIGURAÇÃO DE SEGURANÇA ---

// 1. Adicionar o serviço de Autenticação
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true, // Garante que o token não expirou
        ValidateIssuerSigningKey = true, // Valida a assinatura do token

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// 2. Adicionar o serviço de Autorização (para usar [Authorize])
builder.Services.AddAuthorization();

// --- FIM DA CONFIGURAÇÃO DE SEGURANÇA ---

// --- CONFIGURAÇÃO DOS SERVIÇOS ---

// 1. Serviços padrão da API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Registrar o banco de dados de GERENCIAMENTO (conexão fixa)
var managementConnectionString = builder.Configuration.GetConnectionString("ManagementDbConnection");
//builder.Services.AddDbContext<ManagementDbContext>(options =>
//    options.UseMySql(managementConnectionString, ServerVersion.AutoDetect(managementConnectionString))
//);
builder.Services.AddDbContext<ManagementDbContext>(options =>
    options.UseMySql(managementConnectionString, ServerVersion.AutoDetect(managementConnectionString))
);

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Configurações de senha (exemplo)
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<ManagementDbContext>(); // Liga o Identity ao nosso DbContext

// 3. Registrar os serviços necessários para a lógica multi-tenant
builder.Services.AddHttpContextAccessor(); // Essencial para o TenantService
builder.Services.AddScoped<ITenantService, TenantService>(); // Nosso serviço que encontra a conexão

// 4. Registrar o DbContext do TENANT (conexão dinâmica) - ESTA É A ÚNICA VEZ QUE REGISTRAMOS O SystemContext
builder.Services.AddDbContext<SystemContext>((serviceProvider, options) =>
{
    // Pega o serviço de tenant para a requisição atual
    var tenantService = serviceProvider.GetRequiredService<ITenantService>();

    // Pega a string de conexão específica do tenant
    var tenantConnectionString = tenantService.GetConnectionString();

    // Configura o DbContext para usar essa string de conexão
    options.UseMySql(tenantConnectionString, ServerVersion.AutoDetect(tenantConnectionString));
});

// 5. Registrar outros serviços da sua aplicação
builder.Services.AddScoped<CategoryLogic>();


// --- CONFIGURAÇÃO DO PIPELINE HTTP ---

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Verifica quem é o usuário (lê o token)
app.UseAuthorization();  // Verifica se o usuário tem permissão para acessar o endpoint


// ATENÇÃO: O TenantMiddleware precisa ser removido ou adaptado.
// A lógica dele (identificar o tenant) agora está DENTRO do TenantService.
// Se você mantiver o middleware, ele precisa ser muito simples ou pode causar conflitos.
// app.UseMiddleware<TenantMiddleware>(); // <-- RECOMENDO REMOVER POR ENQUANTO

app.MapControllers();

app.Run();