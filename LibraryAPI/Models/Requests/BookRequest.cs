using LibraryAPI.Validations;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.Requests
{
    public class BookRequest
    {
        [Required]
        [FirstCapitalLetter]
        public required string Title { get; set; }
        [Required]
        public required List<int> AuthorIds { get; set; } = [];
    }
}
