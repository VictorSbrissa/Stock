using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ClienteRequest
    {
        [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
        [StringLength(255)]
        public string Nome { get; set; }

        [StringLength(20)]
        public string? Telefone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }
    }
}
