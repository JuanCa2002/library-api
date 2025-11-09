namespace LibraryAPI.Models.Responses
{
    public class AuthenticationResponse
    {
        public required string Token { get; set; }
        public DateTime ExpireDate { get; set; }
    }
}
