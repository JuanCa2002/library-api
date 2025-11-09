using static System.Net.Mime.MediaTypeNames;

namespace LibraryAPI.Middlewares
{
    public class LogRequestMiddleware
    {
        private readonly RequestDelegate _next;
        public LogRequestMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");

            await _next.Invoke(context);

            logger.LogInformation($"Response: {context.Response.StatusCode}");
        }
        
    }

    public static class LogRequestMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogRequest(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogRequestMiddleware>();
        }
    }
}
