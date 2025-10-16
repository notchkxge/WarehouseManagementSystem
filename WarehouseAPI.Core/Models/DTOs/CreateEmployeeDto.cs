using System.ComponentModel.DataAnnotations;

namespace WarehouseAPI.Core.Models.DTOs
{
    public class CreateEmployeeDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        [Required]
        [MaxLength(100)]
        public string Login { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        public int RoleId { get; set; }
    }
}