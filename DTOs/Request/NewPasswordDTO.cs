using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.DTOs.Request
{
    public class NewPasswordDTO
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
        public string ApplicationUserId { get; set; } = string.Empty;
    }
}
