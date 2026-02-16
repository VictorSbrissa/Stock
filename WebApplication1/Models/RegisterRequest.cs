using System.ComponentModel.DataAnnotations; // Adicione este using para [Required]

namespace WebApplication1.Models
{
    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress] // É uma boa prática adicionar validação de e-mail
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
