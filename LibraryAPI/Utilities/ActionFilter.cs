using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryAPI.Utilities
{
    public class ActionFilter : IActionFilter
    {
        private readonly ILogger<ActionFilter> _logger;

        public ActionFilter(ILogger<ActionFilter> logger)
        {
            _logger = logger;
        }

        // Before the action
        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation("Executing action");
        }
        // After the action
        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("Executed action");
        }
    }
}
