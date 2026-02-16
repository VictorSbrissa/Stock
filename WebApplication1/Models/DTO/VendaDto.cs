namespace WebApplication1.Models.DTO
{
    public class VendaDto
    {
        public int Id { get; set; }
        public DateTime DataVenda { get; set; }
        public decimal ValorTotal { get; set; }
        public StatusPagamento StatusPagamento { get; set; }
        // Sem a propriedade de navegação para Cliente, quebrando o ciclo!
    }

    // Models/DTOs/ClienteDetailsDto.cs
    public class ClienteDetailsDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string? Email { get; set; }
        public bool Ativo { get; set; }
        // A lista de vendas usará o DTO de Venda, não a entidade
        public List<VendaDto> Vendas { get; set; } = new List<VendaDto>();
    }
}
