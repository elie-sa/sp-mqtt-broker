using System.Security.Claims;

namespace SPBackend.Services.CurrentUser;

public class CurrentUser: ICurrentUser
{
    private readonly IHttpContextAccessor _http;

    public CurrentUser(IHttpContextAccessor http) => _http = http;

    public bool IsAuthenticated =>
        _http.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public string? Sub =>
        _http.HttpContext?.User?.FindFirst("sub")?.Value
        ?? _http.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}