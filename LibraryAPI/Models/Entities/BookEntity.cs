using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.Entities
{
    public class BookEntity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Title { get; set; }

        public List<AuthorBookEntity> Authors { get; set; } = [];

        public List<CommentEntity> Comments { get; set; } = [];
    }
}
