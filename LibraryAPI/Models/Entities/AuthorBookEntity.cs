using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Models.Entities
{
    [PrimaryKey(nameof(AuthorId), nameof(BookId))]
    public class AuthorBookEntity
    {
        public int AuthorId { get; set; }
        public int BookId { get; set; }
        public int Order { get; set; }
        public AuthorEntity? Author { get; set; }
        public BookEntity? Book { get; set; }
    }
}
