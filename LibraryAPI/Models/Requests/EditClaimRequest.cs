using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.Requests
{
    public class EditClaimRequest
    {
        [EmailAddress]
        [Required]
        public required string Email { get; set; }
    }
}
