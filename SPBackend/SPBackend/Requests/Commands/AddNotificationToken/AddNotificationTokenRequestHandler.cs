using MediatR;
using SPBackend.Services.Notifications;

namespace SPBackend.Requests.Commands.AddNotificationToken;

public class AddNotificationTokenRequestHandler: IRequestHandler<AddNotificationTokenRequest, AddNotificationTokenResponse>
{
    private readonly NotificationService _notificationService;

    public AddNotificationTokenRequestHandler(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<AddNotificationTokenResponse> Handle(AddNotificationTokenRequest request,
        CancellationToken cancellationToken)
    {
        return await _notificationService.AddNotificationToken(request, cancellationToken);
    }  
}