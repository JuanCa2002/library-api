using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.Entities
{
    public class AuthorEntity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Names { get; set; }

        [Required]
        [StringLength(100)]
        public required string LastNames { get; set; }

        [StringLength(20)]
        public string? Identification { get; set; }

        [Unicode(false)]
        public string? Picture { get; set; }

        public List<AuthorBookEntity> Books { get; set; } = [];
    }
}
