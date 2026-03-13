using Microsoft.EntityFrameworkCore;
using SPBackend.Data;
using SPBackend.Requests.Commands.SendNotification;
using SPBackend.Services.CurrentUser;

namespace SPBackend.Services.Notifications;

public class NotificationHelper
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly NotificationService _notificationService;
    
    public NotificationHelper(IAppDbContext dbContext, ICurrentUser currentUser, NotificationService notificationService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task<SendNotificationResponse> SendNotificationHelper(string title, string body, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId == _currentUser.Sub, cancellationToken: cancellationToken);
        var token = await _dbContext.NotificationTokens.FirstOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);

        if (token == null)
        {
            throw new Exception("User does not have a registered token.");
        }
        
        var request = new SendNotificationRequest
        {
            To = token.Token,
            Title = title,
            Body = body
        };

        var response = await _notificationService.SendNotification(request, cancellationToken);
        return response;
    }
}