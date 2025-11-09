namespace LibraryAPI.Models.Responses
{
    public class AuthorResponse: ResourceResponse
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public string? Picture { get; set; }
    }
}
