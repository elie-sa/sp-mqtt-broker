using Microsoft.EntityFrameworkCore;
using SPBackend.Data;
using SPBackend.Models;
using SPBackend.Requests.Commands.AddNotificationToken;
using SPBackend.Services.CurrentUser;

namespace SPBackend.Services.Notifications;

public class NotificationService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public NotificationService(IAppDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<AddNotificationTokenResponse> AddNotificationToken(AddNotificationTokenRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.KeyCloakId.Equals(_currentUser.Sub), cancellationToken: cancellationToken);

        if (await _dbContext.NotificationTokens.AnyAsync(x => x.UserId == user.Id && x.Token == request.Token))
        {
            return new AddNotificationTokenResponse() { Message = "Token already exists." };
        }
        
        var tokenToAdd = new NotificationToken()
        {
            UserId = user.Id,
            Token = request.Token
        };

        await _dbContext.NotificationTokens.AddAsync(tokenToAdd, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AddNotificationTokenResponse() { Message = "Successfully added token." };
    }
}