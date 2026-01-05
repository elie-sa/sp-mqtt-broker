using MediatR;

namespace SPBackend.Commands.AddTimeout;

public class AddTimeoutRequest: IRequest<AddTimeoutResponse>
{
    public long PlugId { get; set; }
    public TimeSpan Timeout { get; set; }
}