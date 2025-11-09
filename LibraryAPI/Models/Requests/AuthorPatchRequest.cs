using LibraryAPI.Validations;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.Requests
{
    public class AuthorPatchRequest
    {
        [Required]
        [StringLength(100)]
        [FirstCapitalLetter]
        public required string Names { get; set; }

        [Required]
        [StringLength(100)]
        [FirstCapitalLetter]
        public required string LastNames { get; set; }

        [StringLength(20)]
        public string? Identification { get; set; }
    }
}
