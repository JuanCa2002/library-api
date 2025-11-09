using LibraryAPI.Models.Requests;

namespace LibraryAPI.Utilities
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable,
            PaginationRequest paginationRequest)
        {
            return queryable
                .Skip((paginationRequest.Page - 1) * paginationRequest.RecordsPerPage)
                .Take(paginationRequest.RecordsPerPage);
        }
    }
}
