using MediatR;
using SPBackend.Services.Notifications;

namespace SPBackend.Requests.Commands.SendNotification;

public class SendNotificationRequestHandler: IRequestHandler<SendNotificationRequest, SendNotificationResponse>
{
    private readonly NotificationService _notificationService;

    public SendNotificationRequestHandler(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<SendNotificationResponse> Handle(
        SendNotificationRequest request,
        CancellationToken cancellationToken)
    {
        return await _notificationService.SendNotification(request, cancellationToken);
    }
}