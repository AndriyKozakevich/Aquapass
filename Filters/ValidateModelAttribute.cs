using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace AquaPass.Filters;

public class ValidateModelAttribute : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Validate ModelState for complex types
        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
            return;
        }

        // Check action parameters for null complex types
        foreach (var param in context.ActionDescriptor.Parameters)
        {
            if (context.ActionArguments.TryGetValue(param.Name ?? string.Empty, out var value))
            {
                if (value == null)
                {
                    context.Result = new BadRequestObjectResult(new { message = $"Parameter '{param.Name}' is required." });
                    return;
                }
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // noop
    }
}
