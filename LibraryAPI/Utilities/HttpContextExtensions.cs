using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Utilities
{
    public static class HttpContextExtensions
    {
        public async static Task InsertParamsPaginationHeaders<T>(this HttpContext httpContext,
            IQueryable<T> queryable)
        {
            if(httpContext is null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            double quantity = await queryable.CountAsync();

            httpContext.Response.Headers.Append("total-quantity", quantity.ToString());
        }
    }
}
