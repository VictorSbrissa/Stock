using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication1.Infrastructure;
using WebApplication1.Models;
using WebApplication1.Models.DTO;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class VendasController : ControllerBase
{
    private readonly SystemContext _context;

    public VendasController(SystemContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateVenda([FromBody] VendaRequest request)
    {
        // 1. Verificar se o cliente associado à venda existe.
        var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == request.ClienteId);
        if (!clienteExiste)
        {
            return NotFound($"Cliente com o ID {request.ClienteId} não encontrado.");
        }

        // 2. Mapear o DTO para a entidade Venda
        var novaVenda = new Venda
        {
            ClienteId = request.ClienteId,
            ValorTotal = request.ValorTotal,
            ValorPago = request.ValorPago,
            StatusPagamento = request.StatusPagamento,
            DataVenda = DateTime.UtcNow // Define a data da venda para o momento da criação
        };

        // 3. Adicionar e salvar no banco de dados
        _context.Vendas.Add(novaVenda);
        await _context.SaveChangesAsync();

        // 4. Retornar a venda criada
        // (Vamos criar um endpoint GetVenda a seguir para que o CreatedAtAction funcione)
        return CreatedAtAction(nameof(GetVenda), new { id = novaVenda.Id }, novaVenda);
    }
    [HttpGet]
    public async Task<IActionResult> GetVendas()
    {
        var vendas = await _context.Vendas.ToListAsync();
        return Ok(vendas);
    }

    // GET: api/Vendas/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVenda(int id)
    {


        var venda = await _context.Vendas
                               .Include(v => v.Cliente)
                               .FirstOrDefaultAsync(v => v.Id == id);

        if (venda == null)
        {
            return NotFound($"Venda com o ID {id} não encontrado.");
        }

        var vendaDto = new VendaDetailsDto
        {
            Id = venda.Id,
            DataVenda = venda.DataVenda,
            ValorTotal = venda.ValorTotal,
            StatusPagamento = venda.StatusPagamento,
            Cliente = new ClienteSummaryDto
            {
                Id = venda.Cliente.Id,
                Nome = venda.Cliente.Nome
            }
        };

        return Ok(vendaDto);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVenda(int id, [FromBody] VendaRequest request)
    {
        var vendaToUpdate = await _context.Vendas.FindAsync(id);

        if (vendaToUpdate == null)
        {
            return NotFound($"Venda com o ID {id} não encontrada.");
        }

        // Verificar se o cliente existe, caso o ID do cliente seja alterado
        var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == request.ClienteId);
        if (!clienteExiste)
        {
            return NotFound($"Cliente com o ID {request.ClienteId} não encontrado para associar à venda.");
        }

        // Atualiza as propriedades
        vendaToUpdate.ClienteId = request.ClienteId;
        vendaToUpdate.ValorTotal = request.ValorTotal;
        vendaToUpdate.ValorPago = request.ValorPago;
        vendaToUpdate.StatusPagamento = request.StatusPagamento;

        await _context.SaveChangesAsync();

        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVenda(int id)
    {
        var vendaToDelete = await _context.Vendas.FindAsync(id);

        if (vendaToDelete == null)
        {
            return NotFound($"Venda com o ID {id} não encontrada.");
        }

        _context.Vendas.Remove(vendaToDelete);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
