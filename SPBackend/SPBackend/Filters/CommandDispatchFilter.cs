using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using SPBackend.Services.Commands;

namespace SPBackend.Filters;

public sealed class CommandDispatchFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> SupportedMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "POST",
        "PUT"
    };

    private readonly CommandDispatcher _dispatcher;
    private readonly CommandInboxOptions _options;
    private readonly ILogger<CommandDispatchFilter> _logger;

    public CommandDispatchFilter(
        CommandDispatcher dispatcher,
        IOptions<CommandInboxOptions> options,
        ILogger<CommandDispatchFilter> logger)
    {
        _dispatcher = dispatcher;
        _options = options.Value;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var method = context.HttpContext.Request.Method;
        if (!SupportedMethods.Contains(method))
        {
            await next();
            return;
        }

        var executed = await next();

        if (executed.Exception != null && !executed.ExceptionHandled)
        {
            return;
        }

        if (!IsSuccessResult(executed.Result))
        {
            return;
        }

        var payload = BuildPayload(context.ActionArguments);
        var path = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
        var userId = context.HttpContext.User?.FindFirst("sub")?.Value
            ?? context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var waitForAck = method.Equals("POST", StringComparison.OrdinalIgnoreCase);
        var ackTimeout = TimeSpan.FromSeconds(Math.Max(1, _options.AckTimeoutSeconds));

        var dispatchResult = await _dispatcher.DispatchAsync(
            method,
            path,
            payload,
            userId,
            waitForAck,
            ackTimeout,
            context.HttpContext.RequestAborted);

        if (waitForAck && !dispatchResult.Acknowledged)
        {
            _logger.LogWarning("Command {CommandId} did not receive acknowledgment in time.", dispatchResult.CommandId);
            executed.Result = new ObjectResult(new { message = "Command acknowledgment timed out." })
            {
                StatusCode = StatusCodes.Status504GatewayTimeout
            };
        }
    }

    private static bool IsSuccessResult(IActionResult? result)
    {
        if (result == null)
        {
            return true;
        }

        return result switch
        {
            ObjectResult objectResult => objectResult.StatusCode is null or < 400,
            StatusCodeResult statusCodeResult => statusCodeResult.StatusCode < 400,
            _ => true
        };
    }

    private static object BuildPayload(IDictionary<string, object?> arguments)
    {
        if (arguments.Count == 0)
        {
            return new { };
        }

        var payload = new Dictionary<string, object?>();
        foreach (var (key, value) in arguments)
        {
            if (value is CancellationToken)
            {
                continue;
            }

            payload[key] = value;
        }

        if (payload.Count == 1)
        {
            return payload.Values.First() ?? new { };
        }

        return payload;
    }
}
