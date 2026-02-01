using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class CreateTenantRequest
    {
        [Required]
        [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "O Tenant ID deve conter apenas letras minúsculas, números e hifens.")]
        public string TenantId { get; set; }
    }
}
