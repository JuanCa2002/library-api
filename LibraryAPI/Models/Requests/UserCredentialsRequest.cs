using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.Requests
{
    public class UserCredentialsRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public string? Password { get; set; }

    }
}
