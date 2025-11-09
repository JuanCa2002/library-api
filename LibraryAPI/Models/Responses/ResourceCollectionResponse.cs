namespace LibraryAPI.Models.Responses
{
    public class ResourceCollectionResponse<T>:ResourceResponse where T: ResourceResponse
    {
        public IEnumerable<T> Items { get; set; } = [];
    }
}
