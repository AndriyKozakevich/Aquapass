using Microsoft.AspNetCore.Mvc;
using System;

namespace AquaPass.Extensions;

public static class ControllerExtensions
{
    public static ActionResult? ValidateId(this ControllerBase controller, Guid id, string paramName = "id")
    {
        if (id == Guid.Empty)
        {
            return controller.BadRequest(new { message = $"Parameter '{paramName}' is required and must be a valid GUID." });
        }

        return null;
    }

    public static ActionResult? ValidateStringParam(this ControllerBase controller, string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return controller.BadRequest(new { message = $"Parameter '{paramName}' is required and cannot be empty." });
        }

        return null;
    }
}
