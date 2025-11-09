using LibraryAPI.Models.Responses;

namespace LibraryAPI.Services.V1
{
    public interface ILinksGenerator
    {
        Task GenerateLinks(AuthorResponse author);
        Task<ResourceCollectionResponse<AuthorResponse>> GenerateLinks(List<AuthorResponse> author);
    }
}
