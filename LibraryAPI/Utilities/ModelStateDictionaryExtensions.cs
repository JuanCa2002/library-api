using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LibraryAPI.Utilities
{
    public static class ModelStateDictionaryExtensions
    {
        public static BadRequestObjectResult BuildProblemDetail(
            this ModelStateDictionary modelState)
        {
            var problemDetails = new ValidationProblemDetails(modelState)
            {
                Title = "One or more validation errors occurred",
                Status = StatusCodes.Status400BadRequest
            };

            return new BadRequestObjectResult(problemDetails);
        }
    }
}
