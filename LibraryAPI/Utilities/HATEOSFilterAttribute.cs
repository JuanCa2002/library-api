using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryAPI.Utilities
{
    public class HATEOSFilterAttribute: ResultFilterAttribute
    {
        protected bool MustIncludeHATEOS(ResultExecutingContext context)
        {
            if (context.Result is not ObjectResult result || !IsSuccessful(result)) 
            {
                return false;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue("IncludeHATEOS", out var header))
            {
                return false;
            }

            return string.Equals(header, "Y", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsSuccessful(ObjectResult result)
        {
            if(result.Value is null)
            {
                return false;
            }

            if(result.StatusCode.HasValue && !result.StatusCode.Value.ToString().StartsWith("2"))
            {
                return false;
            }

            return true;
        }
    }
}
