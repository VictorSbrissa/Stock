using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication1.Infrastructure; // Onde está o SystemContext
using WebApplication1.Models;       // Onde está a Category
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]

    public class CategoriesController : ControllerBase
    {
        private readonly SystemContext _context;

        // O SystemContext é injetado aqui. Graças à nossa configuração no Program.cs,
        // ele já estará conectado ao banco de dados correto do tenant!
        public CategoriesController(SystemContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            // Este comando será executado no banco de dados do tenant que fez a requisição.
            var categories = await _context.Category.ToListAsync();
            return Ok(categories);
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("O nome da categoria é obrigatório.");
            }

            var newCategory = new Category { Name = request.Name };

            // O EF Core adiciona a nova categoria ao banco de dados do tenant atual.
            _context.Category.Add(newCategory);
            await _context.SaveChangesAsync();

            // Retorna a categoria criada com seu novo ID.
            return CreatedAtAction(nameof(GetCategories), new { id = newCategory.Id }, newCategory);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            // 1. Encontrar a categoria pelo ID fornecido na URL.
            // Usamos FindAsync, que é otimizado para busca por chave primária.
            var categoryToDelete = await _context.Category.FindAsync(id);

            // 2. Verificar se a categoria foi encontrada.
            if (categoryToDelete == null)
            {
                // Se não encontrarmos, retornamos 404 Not Found, que é o código
                // HTTP correto para um recurso que não existe.
                return NotFound($"Categoria com o ID {id} não encontrada.");
            }

            // 3. Se encontrada, marcamos para remoção.
            _context.Category.Remove(categoryToDelete);

            // 4. Salvamos as alterações no banco de dados para efetivar a exclusão.
            await _context.SaveChangesAsync();

            // 5. Retornamos 204 No Content, o código de sucesso padrão para uma
            // operação de DELETE que não precisa retornar nenhum dado.
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryRequest request)
        {
            // 1. Validação básica dos dados de entrada
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("O nome da categoria é obrigatório.");
            }

            // 2. Encontrar a categoria existente no banco de dados pelo ID da URL.
            var categoryToUpdate = await _context.Category.FindAsync(id);

            // 3. Verificar se a categoria foi encontrada.
            if (categoryToUpdate == null)
            {
                // Se não, o recurso a ser atualizado não existe. Retornamos 404 Not Found.
                return NotFound($"Categoria com o ID {id} não encontrada.");
            }

            // 4. Atualizar as propriedades da entidade encontrada com os dados da requisição.
            // Esta é a essência da operação de atualização.
            categoryToUpdate.Name = request.Name;
            // Se a entidade Category tivesse mais propriedades (ex: Descricao),
            // nós as atualizaríamos aqui também.
            // categoryToUpdate.Descricao = request.Descricao;

            // O Entity Framework é inteligente. Ao modificar a entidade 'categoryToUpdate',
            // ele automaticamente a marca como 'Modified'.

            // 5. Salvar as alterações no banco de dados.
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Esta é uma verificação de segurança avançada. Se outro usuário deletou
                // a categoria entre o nosso FindAsync e o SaveChangesAsync, o EF lançará
                // esta exceção. Nesse caso, o recurso não existe mais.
                return NotFound($"Erro de concorrência. A categoria com o ID {id} pode ter sido removida.");
            }

            // 6. Retornar 204 No Content, indicando que a atualização foi bem-sucedida
            // e que não há necessidade de retornar o objeto no corpo da resposta.
            // Alternativamente, você poderia retornar Ok(categoryToUpdate) se quisesse
            // enviar o objeto atualizado de volta para o cliente.
            return NoContent();
        }
    }

    // Um modelo simples para a requisição POST, para não expor a entidade completa.
    public class CategoryRequest
    {
        public string Name { get; set; }
    }
}
