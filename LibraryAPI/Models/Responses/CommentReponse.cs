using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.Responses
{
    public class CommentReponse
    {
        public Guid Id { get; set; }
        public required string Body { get; set; }
        public DateTime PublishedDate { get; set; }
        public required string UserId { get; set; }
        public required string UserEmail { get; set; }
    }
}
