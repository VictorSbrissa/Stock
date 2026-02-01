using SeuProjeto.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    // Enum para o Status do Pagamento
    public enum StatusPagamento
    {
        PENDENTE,
        PAGO,
        CANCELADO
    }

    [Table("Vendas")]
    public class Venda
    {
        [Key]
        [Column("idVenda")]
        public int Id { get; set; }

        public DateTime DataVenda { get; set; }

        [Column(TypeName = "decimal(10, 2)")] // Especifica o tipo de coluna do banco
        public decimal ValorPago { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal ValorTotal { get; set; }

        [Required]
        public StatusPagamento StatusPagamento { get; set; }

        // Chave Estrangeira e Propriedade de Navegação
        [Column("idCliente")]
        public int ClienteId { get; set; } // Coluna da chave estrangeira

        [ForeignKey("ClienteId")] // Aponta para a propriedade que contém a chave estrangeira
        public virtual Cliente Cliente { get; set; } // Propriedade de navegação para o cliente relacionado
    }
}
