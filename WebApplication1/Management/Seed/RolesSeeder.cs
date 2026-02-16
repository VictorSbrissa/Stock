using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace WebApplication1.Management.Seed
{
    public static class RolesSeeder
    {
        // Este método será chamado na inicialização da aplicação
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            // Lista de roles que queremos garantir que existam
            string[] roleNames = { "Admin", "UsuarioComum" };

            foreach (var roleName in roleNames)
            {
                // Verifica se a role já existe no banco de dados
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    // Se não existir, cria a role
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}