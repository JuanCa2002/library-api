using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.Requests
{
    public class BookUpdateRequest: BookRequest
    {
        [Required]
        public required int Id { get; set; }
    }
}
