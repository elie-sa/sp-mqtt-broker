using MediatR;

namespace SPBackend.Requests.Commands.AddTimeout;

public class AddTimeoutRequest: IRequest<AddTimeoutResponse>
{
    public long PlugId { get; set; }
    public TimeSpan Timeout { get; set; }
}