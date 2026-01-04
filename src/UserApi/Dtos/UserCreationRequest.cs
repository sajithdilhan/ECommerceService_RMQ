using Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace UserApi.Dtos
{
    public class UserCreationRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public User MapToUser()
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Name = this.Name,
                Email = this.Email
            };
        }
    }
}
