namespace LibraryAPI.Models.Entities
{
    public class ErrorEntity
    {
        public Guid Id { get; set; }
        public required string Message { get; set; }
        public string? StackTrace { get; set; }
        public DateTime Date { get; set; }
    }
}
