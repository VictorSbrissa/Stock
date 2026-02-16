namespace WebApplication1.Models.DTO
{
    public class VendaDetailsDto
    {
        public int Id { get; set; }
        public DateTime DataVenda { get; set; }
        public decimal ValorTotal { get; set; }
        public StatusPagamento StatusPagamento { get; set; }

        // Inclui o DTO do cliente, não a entidade completa
        public ClienteSummaryDto Cliente { get; set; }
    }
}
