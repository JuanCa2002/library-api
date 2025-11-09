namespace LibraryAPI.Models.Responses
{
    public class AuthorWithBooksResponse: AuthorResponse
    {
        public List<BookSimpleResponse> Books { get; set; } = [];
    }
}
