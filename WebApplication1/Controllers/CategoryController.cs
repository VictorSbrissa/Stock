using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication1.Infrastructure; // Onde está o SystemContext
using WebApplication1.Models;       // Onde está a Category

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
    }

    // Um modelo simples para a requisição POST, para não expor a entidade completa.
    public class CategoryRequest
    {
        public string Name { get; set; }
    }
}
