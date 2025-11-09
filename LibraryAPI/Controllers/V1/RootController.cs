using LibraryAPI.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1")]
    [Authorize]
    public class RootController: ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        public RootController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [HttpGet(Name = "GetRootV1")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<HATEOSDataResponse>>> Get()
        {
            var HATEOSData = new List<HATEOSDataResponse>();

            var isAdmin = await _authorizationService.AuthorizeAsync(User, "isAdmin");

            // Actions without restrictions

            HATEOSData.Add(new HATEOSDataResponse(Link: Url.Link("GetRootV1", new { })!,
                Description: "self", Method: "GET"));

            HATEOSData.Add(new HATEOSDataResponse(Link: Url.Link("GetAllAuthorsV1", new { })!,
                Description: "get-authors", Method: "GET"));

            HATEOSData.Add(new HATEOSDataResponse(Link: Url.Link("RegisterUserV1", new { })!,
                Description: "create-user", Method: "POST"));

            HATEOSData.Add(new HATEOSDataResponse(Link: Url.Link("LoginUserV1", new { })!,
                Description: "login-user", Method: "POST"));

            // Actions that require that the user is authenticated

            if (User.Identity!.IsAuthenticated)
            {
                HATEOSData.Add(new HATEOSDataResponse(Link: Url.Link("UpdateUserV1", new { })!,
                    Description: "update-user", Method: "PUT"));

                HATEOSData.Add(new HATEOSDataResponse(Link: Url.Link("RenewTokenV1", new { })!,
                    Description: "renew-token", Method: "GET"));
            }

            // Actions that only admin users can do

            if (isAdmin.Succeeded)
            {
                HATEOSData.Add(new HATEOSDataResponse(Link: Url.Link("CreateAuthorV1", new { })!,
                    Description: "create-author", Method: "POST"));

                HATEOSData.Add(new HATEOSDataResponse(Link: Url.Link("CreateAuthorsV1", new { })!,
                    Description: "create-authors", Method: "POST"));

                HATEOSData.Add(new HATEOSDataResponse(Link: Url.Link("CreateBookV1", new { })!,
                    Description: "create-book", Method: "POST"));

                HATEOSData.Add(new HATEOSDataResponse(Link: Url.Link("GetAllUsersV1", new { })!,
                    Description: "get-users", Method: "GET"));
            }

            return HATEOSData;
        }
    }
}
