using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.Entities
{
    public class CommentEntity
    {
        public Guid Id { get; set; }
        [Required]
        public required string Body { get; set; }

        public DateTime PublishedDate { get; set; }
        public int BookId { get; set; }
        public BookEntity? Book { get; set; }
        public required string UserId { get; set; }
        public bool IsDeleted { get; set; }
        public UserEntity? User { get; set; }
    }
}
