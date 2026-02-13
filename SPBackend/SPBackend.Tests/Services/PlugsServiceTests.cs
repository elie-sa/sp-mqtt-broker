using System.Globalization;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Moq;
using SPBackend.Data;
using SPBackend.Models;
using SPBackend.Requests.Commands.AddTimeout;
using SPBackend.Services.CurrentUser;
using SPBackend.Services.Mqtt;
using SPBackend.Services.Plugs;

namespace SPBackend.Tests.Services;

public class PlugsServiceTests
{
    [Fact]
    public async Task AddTimeout_SavesTimeout_AndPublishes()
    {
        var plug = new Plug { Id = 1, Name = "Plug", IsOn = false };
        var plugs = new List<Plug> { plug }.AsQueryable();
        var plugDbSet = new Mock<DbSet<Plug>>();
        plugDbSet.As<IQueryable<Plug>>().Setup(m => m.Provider).Returns(plugs.Provider);
        plugDbSet.As<IQueryable<Plug>>().Setup(m => m.Expression).Returns(plugs.Expression);
        plugDbSet.As<IQueryable<Plug>>().Setup(m => m.ElementType).Returns(plugs.ElementType);
        plugDbSet.As<IQueryable<Plug>>().Setup(m => m.GetEnumerator()).Returns(() => plugs.GetEnumerator());

        var dbContext = new Mock<IAppDbContext>();
        dbContext.Setup(c => c.Plugs).Returns(plugDbSet.Object);
        dbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var currentUser = new Mock<ICurrentUser>();
        var mqtt = new Mock<IMqttService>();
        var backgroundJobs = new Mock<IBackgroundJobClient>();

        var service = new PlugsService(dbContext.Object, currentUser.Object, mqtt.Object, backgroundJobs.Object);

        var request = new AddTimeoutRequest
        {
            PlugId = 1,
            Timeout = TimeSpan.FromSeconds(90)
        };

        var response = await service.AddTimeout(request, CancellationToken.None);

        Assert.Equal(TimeSpan.FromSeconds(90), plug.Timeout);
        mqtt.Verify(m => m.ConnectAsync(), Times.Once);
        mqtt.Verify(m => m.PublishAsync("home/plug/1/timeout", request.Timeout.TotalSeconds.ToString(CultureInfo.InvariantCulture)), Times.Once);
    }
}
