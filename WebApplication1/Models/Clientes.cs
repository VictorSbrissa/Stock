using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models;

namespace SeuProjeto.Models
{
    [Table("Clientes")] // Define o nome da tabela explicitamente
    public class Cliente
    {
        [Key] // Marca como chave primária
        [Column("idCliente")] // Mapeia para o nome da coluna
        public int Id { get; set; }

        [Required] // Corresponde ao NOT NULL
        [StringLength(255)]
        public string Nome { get; set; }

        [StringLength(20)]
        public string? Telefone { get; set; } // O '?' torna a string opcional (nulável)

        [StringLength(100)]
        public string? Email { get; set; }

        // O EF Core gerencia colunas de data/hora de atualização e criação
        // de forma mais elegante com interceptors ou na própria lógica de negócio.
        // Por simplicidade, vamos declará-las, mas o banco de dados cuidará dos valores padrão.
        public DateTime AtualizadoEm { get; set; }
        public DateTime CriadoEm { get; set; }

        public bool Ativo { get; set; }

        // Propriedade de navegação: um cliente pode ter várias vendas
        public virtual ICollection<Venda> Vendas { get; set; } = new List<Venda>();
    }
}