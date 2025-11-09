using Azure;
using LibraryAPI.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using System;

namespace LibraryAPI.Services.V1
{
    public class LinksGenerator: ILinksGenerator
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LinksGenerator(LinkGenerator linkGenerator, 
            IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
        {
            _linkGenerator = linkGenerator;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResourceCollectionResponse<AuthorResponse>> GenerateLinks(List<AuthorResponse> authors)
        {
            var result = new ResourceCollectionResponse<AuthorResponse> { Items = authors };

            var user = _httpContextAccessor.HttpContext!.User;
            var isAdmin = await _authorizationService.AuthorizeAsync(user, "isAdmin");

            foreach (var author in authors)
            {
                GenerateLinks(author, isAdmin.Succeeded);
            }

            result.Links.Add(new HATEOSDataResponse(
                  Link: _linkGenerator.GetUriByRouteValues(
                _httpContextAccessor.HttpContext!, "GetAllAuthorsV1", new { })!,
                  Description: "self",
                  Method: "GET"));

            if (isAdmin.Succeeded) 
            {
                result.Links.Add(new HATEOSDataResponse(
                  Link: _linkGenerator.GetUriByRouteValues(
                _httpContextAccessor.HttpContext!, "CreateAuthorV1", new { })!,
                  Description: "create-author",
                  Method: "POST"));

                result.Links.Add(new HATEOSDataResponse(
                  Link: _linkGenerator.GetUriByRouteValues(
                _httpContextAccessor.HttpContext!, "CreateAuthorWithPictureV1", new { })!,
                  Description: "create-author-with-picture",
                  Method: "POST"));
            }

            return result;
        }

        public async Task GenerateLinks(AuthorResponse author)
        {
            var user = _httpContextAccessor.HttpContext!.User;
            var isAdmin = await _authorizationService.AuthorizeAsync(user, "isAdmin");
            GenerateLinks(author, isAdmin.Succeeded);
        }

        private void GenerateLinks(AuthorResponse response, bool isAdmin)
        {
            response.Links.Add(new HATEOSDataResponse(Link: _linkGenerator.GetUriByRouteValues(
                _httpContextAccessor.HttpContext!,"GetAuthorV1",
                new { id = response.Id })!, Description: "self", Method: "GET"));

            if (isAdmin)
            {
                response.Links.Add(new HATEOSDataResponse(Link: _linkGenerator.GetUriByRouteValues(
                _httpContextAccessor.HttpContext!,"UpdateAuthorV1",
                    new { id = response.Id })!, Description: "update-author", Method: "PUT"));

                response.Links.Add(new HATEOSDataResponse(Link: _linkGenerator.GetUriByRouteValues(
                _httpContextAccessor.HttpContext!, "UpdateAttributesAuthorV1",
                    new { id = response.Id })!, Description: "patch-author", Method: "PATCH"));

                response.Links.Add(new HATEOSDataResponse(Link: _linkGenerator.GetUriByRouteValues(
                _httpContextAccessor.HttpContext!, "DeleteAuthorV1",
                    new { id = response.Id })!, Description: "delete-author", Method: "DELETE"));
            }
        }
    }
}
