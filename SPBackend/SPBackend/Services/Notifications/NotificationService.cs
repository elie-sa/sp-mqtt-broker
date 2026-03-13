using Microsoft.EntityFrameworkCore;
using SPBackend.Data;
using SPBackend.Models;
using SPBackend.Requests.Commands.AddNotificationToken;
using SPBackend.Requests.Commands.SendNotification;
using SPBackend.Services.CurrentUser;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using SPBackend.DTOs;

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

    public async Task<SendNotificationResponse> SendNotification(SendNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var client = new HttpClient();
        
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://exp.host/--/api/v2/push/send", content, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Expo push request failed: {response.StatusCode}");
        }
        
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var ticketResponse = JsonSerializer.Deserialize<ExpoPushTicketResponseDto>(responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (ticketResponse.Data.Status == "error")
        {
            throw new Exception(ticketResponse.Data.Message); 
        }
        
        return new SendNotificationResponse() { Message = "Successfully sent the notification." };
    }
}