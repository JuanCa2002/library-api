using LibraryAPI.Models.Responses;
using LibraryAPI.Services.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryAPI.Utilities.V1
{
    public class HATEOSAuthorsAttribute: HATEOSFilterAttribute
    {
        private readonly ILinksGenerator _linksGenerator;

        public HATEOSAuthorsAttribute(ILinksGenerator linksGenerator)
        {
            _linksGenerator = linksGenerator;
        }

        public override async Task OnResultExecutionAsync(
            ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var includeHATEOS = MustIncludeHATEOS(context);

            if (!includeHATEOS)
            {
                await next();
                return;
            }

            var result = context.Result as ObjectResult;
            var model = result!.Value as List<AuthorResponse> ??
                throw new ArgumentNullException("Waiting an instance of list AuthorResponse");

            context.Result = new OkObjectResult(await _linksGenerator.GenerateLinks(model));
            await next();
        }
    }
}
