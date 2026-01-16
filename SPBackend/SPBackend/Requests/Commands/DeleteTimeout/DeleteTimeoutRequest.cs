using MediatR;

namespace SPBackend.Requests.Commands.DeleteTimeout;

public class DeleteTimeoutRequest: IRequest<DeleteTimeoutResponse>
{
    public long PlugId { get; set; }
}