using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class AssignRoleRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string RoleName { get; set; }
    }
}
