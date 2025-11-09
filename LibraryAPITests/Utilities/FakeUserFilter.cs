using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace LibraryAPITests.Utilities
{
    public class FakeUserFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Before the action

            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
                new List<Claim>
                {
                    new Claim("email", "example@email.com")
                }, "test"));

            await next();

            // After the action
        }
    }
}
