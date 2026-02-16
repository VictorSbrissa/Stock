using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeuProjeto.Models;
using System.Threading.Tasks;
using WebApplication1.Infrastructure; // Onde está o SystemContext
using WebApplication1.Models;
using WebApplication1.Models.DTO;       // Onde estão os modelos Cliente, etc.

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]

public class ClientesController : ControllerBase
{
    private readonly SystemContext _context;

    // O SystemContext já vem conectado ao banco do tenant correto.
    public ClientesController(SystemContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetClientes()
    {
        var clientes = await _context.Clientes.ToListAsync();
        return Ok(clientes);
    }

    // GET: api/Clientes/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCliente(int id)
    {
        var cliente = await _context.Clientes
                                .Include(c => c.Vendas)
                                .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null)
        {
            return NotFound($"Cliente com o ID {id} não encontrado.");
        }

        // Mapeamento manual da Entidade para o DTO
        var clienteDto = new ClienteDetailsDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Email = cliente.Email,
            Ativo = cliente.Ativo,
            Vendas = cliente.Vendas.Select(venda => new VendaDto
            {   
                Id = venda.Id,
                DataVenda = venda.DataVenda,
                ValorTotal = venda.ValorTotal,
                StatusPagamento = venda.StatusPagamento
            }).ToList()
        };

        return Ok(clienteDto);
    }
    [HttpPost]
    public async Task<IActionResult> CreateCliente([FromBody] ClienteRequest request)
    {
        // O ASP.NET Core já valida os atributos [Required], [EmailAddress], etc.
        // e retorna um 400 Bad Request se eles falharem.

        var novoCliente = new Cliente
        {
            Nome = request.Nome,
            Telefone = request.Telefone,
            Email = request.Email,
            // O banco de dados cuidará dos valores padrão para CriadoEm, AtualizadoEm e Ativo
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow,
            Ativo = true
        };

        _context.Clientes.Add(novoCliente);
        await _context.SaveChangesAsync();

        // Retorna 201 Created com a localização do novo recurso e o objeto criado.
        return CreatedAtAction(nameof(GetCliente), new { id = novoCliente.Id }, novoCliente);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCliente(int id, [FromBody] ClienteRequest request)
    {
        var clienteToUpdate = await _context.Clientes.FindAsync(id);

        if (clienteToUpdate == null)
        {
            return NotFound($"Cliente com o ID {id} não encontrado.");
        }

        // Atualiza as propriedades da entidade com os dados da requisição
        clienteToUpdate.Nome = request.Nome;
        clienteToUpdate.Telefone = request.Telefone;
        clienteToUpdate.Email = request.Email;
        clienteToUpdate.AtualizadoEm = DateTime.UtcNow; // Atualiza a data de modificação

        await _context.SaveChangesAsync();

        return NoContent(); // Resposta padrão para um PUT bem-sucedido
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCliente(int id)
    {
        var clienteToDelete = await _context.Clientes.FindAsync(id);

        if (clienteToDelete == null)
        {
            return NotFound($"Cliente com o ID {id} não encontrado.");
        }

        // Verificação de segurança: Não permitir deletar um cliente que tenha vendas associadas.
        var hasVendas = await _context.Vendas.AnyAsync(v => v.ClienteId == id);
        if (hasVendas)
        {
            return BadRequest("Não é possível excluir um cliente que possui vendas registradas.");
        }

        _context.Clientes.Remove(clienteToDelete);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
