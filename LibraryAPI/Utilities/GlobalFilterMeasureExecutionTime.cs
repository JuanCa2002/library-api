using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace LibraryAPI.Utilities
{
    public class GlobalFilterMeasureExecutionTime : IAsyncActionFilter
    {
        private readonly ILogger _logger;
        public GlobalFilterMeasureExecutionTime(ILogger<GlobalFilterMeasureExecutionTime> logger)
        {
            _logger = logger;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Before action execution
            var stopWatch = Stopwatch.StartNew();
            _logger.LogInformation($"START ACTION: {context.ActionDescriptor.DisplayName}");

            await next();

            // After action execution
            stopWatch.Stop();
            _logger.LogInformation($"END ACTION: {context.ActionDescriptor.DisplayName} - TIME: {stopWatch.ElapsedMilliseconds} milliseconds");
        }
    }
}
