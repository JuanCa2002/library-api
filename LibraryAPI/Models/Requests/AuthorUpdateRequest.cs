using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.Requests
{
    public class AuthorUpdateRequest: AuthorRequest
    {
        [Required]
        public required int Id { get; set; }

        public IFormFile? Picture { get; set; }
    }
}
