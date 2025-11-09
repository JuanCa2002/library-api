using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.Requests
{
    public class CommentRequest
    {
        [Required]
        public required string Body { get; set; }
    }
}
