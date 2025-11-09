using Azure.Core;
using LibraryAPI.Configuration;
using LibraryAPI.Models.Requests;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Utilities
{
    public class ValidationBookFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _dbContext;
        public ValidationBookFilter(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.TryGetValue("request", out var value) ||
                value is not BookRequest request)
            {
                context.ModelState.AddModelError(string.Empty, "El modelo enviado no es valido");
                context.Result = context.ModelState.BuildProblemDetail();
                return;
            }
            if (request.AuthorIds is null || request.AuthorIds.Count == 0)
            {
                context.ModelState.AddModelError(nameof(request.AuthorIds),
                    "No se puede crear un libro sin autores");
                context.Result = context.ModelState.BuildProblemDetail();
                return;
            }

            var authorExistIds = await _dbContext.Authors
                 .Where(author => request.AuthorIds.Contains(author.Id))
                 .Select(author => author.Id).ToListAsync();

            if (authorExistIds.Count != request.AuthorIds.Count)
            {
                var doesNotExistsAuthors = request.AuthorIds.Except(authorExistIds)
                    .ToList();
                var doesNotExistsAuthorsString = string.Join(",", doesNotExistsAuthors);
                var errorMessage = $"Los siguientes autores no existen: {doesNotExistsAuthorsString}";
                context.ModelState.AddModelError(nameof(request.AuthorIds), errorMessage);
                context.Result = context.ModelState.BuildProblemDetail();
                return;
            }

            await next();
        }
    }
}
