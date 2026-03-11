using MediatR;

namespace SPBackend.Requests.Commands.AddNotificationToken;

public class AddNotificationTokenRequest: IRequest<AddNotificationTokenResponse>
{
    public string Token { get; set; }
}