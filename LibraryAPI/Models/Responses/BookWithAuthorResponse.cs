namespace LibraryAPI.Models.Responses
{
    public class BookWithAuthorResponse: BookSimpleResponse
    {
        public required List<AuthorResponse> Authors { get; set; }
    }
}
