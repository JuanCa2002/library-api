namespace LibraryAPI.Models.Requests
{
    public class AuthorFilterRequest
    {
        public int Page { get; set; } = 1;
        public int RecordPerPage { get; set; } = 10;
        public PaginationRequest Pagination 
        { 
            get
            {
                return new PaginationRequest(Page, RecordPerPage);
            } 
        }
        public string? Names { get; set; }
        public string? LastNames {  get; set; }
        public bool? HasPicture { get; set; }
        public bool? HasBooks { get; set; }
        public string? BookTitle { get; set; }
        public bool IncludeBooks { get; set; }
        public string? OrderField { get; set; }
        public bool AscendantOrder { get; set; } = true;

    }
}
