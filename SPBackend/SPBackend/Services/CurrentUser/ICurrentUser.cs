namespace SPBackend.Services.CurrentUser;

public interface ICurrentUser
{
    string? Sub { get; }
    bool IsAuthenticated { get; }
}