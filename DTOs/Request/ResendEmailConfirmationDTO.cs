using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.DTOs.Request
{
    public class ResendEmailConfirmationDTO
    {
        [Required]
        public string EmailOrUserName { get; set; } = string.Empty;
    }
}
