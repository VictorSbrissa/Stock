using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class VendaRequest
    {
        [Required]
        public int ClienteId { get; set; } // O ID do cliente para quem a venda foi feita

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor total deve ser positivo.")]
        public decimal ValorTotal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "O valor pago não pode ser negativo.")]
        public decimal ValorPago { get; set; } = 0; // Valor padrão de 0

        [Required]
        public StatusPagamento StatusPagamento { get; set; } = StatusPagamento.PENDENTE; // Padrão
    }
}
