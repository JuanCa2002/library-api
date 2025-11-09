namespace LibraryAPI.Models.Requests
{
    public class AuthorWithPictureRequest: AuthorRequest
    {
        public IFormFile? Picture { get; set; }
    }
}
