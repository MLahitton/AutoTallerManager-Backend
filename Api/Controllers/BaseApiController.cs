using Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public abstract class BaseApiController : ControllerBase
{
    protected IActionResult FromResult(Result result, Func<IActionResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);

        return result.IsFailure
            ? MapError(result.Error)
            : onSuccess();
    }

    protected IActionResult FromResult<T>(Result<T> result, Func<T, IActionResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);

        return result.IsFailure
            ? MapError(result.Error)
            : onSuccess(result.Value!);
    }

    private IActionResult MapError(Error error)
    {
        var body = new
        {
            code = error.Code,
            message = error.Message
        };

        if (EndsWith(error.Code, "NotFound"))
        {
            return NotFound(body);
        }

        if (EndsWithAny(error.Code, "Required", "TooShort", "TooLong", "Invalid", "Validation"))
        {
            return BadRequest(body);
        }

        if (EndsWithAny(error.Code, "AlreadyExists", "InUse", "Conflict"))
        {
            return Conflict(body);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, body);
    }

    private static bool EndsWith(string value, string suffix)
    {
        return value.EndsWith(suffix, StringComparison.Ordinal);
    }

    private static bool EndsWithAny(string value, params string[] suffixes)
    {
        return suffixes.Any(suffix => EndsWith(value, suffix));
    }
}
