using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryAPI.Utilities
{
    public class AddHeadersFilterAttribute: ActionFilterAttribute
    {
        private readonly string _name;
        private readonly string _value;
        public AddHeadersFilterAttribute(string name, string value)
        {
            _name = name;
            _value = value;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            // Before action execution
            context.HttpContext.Response.Headers.Append(_name, _value);

            base.OnResultExecuting(context);
            // After action execution
        }
    }
}
