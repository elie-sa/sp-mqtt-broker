using MediatR;

namespace SPBackend.Commands.DeleteTimeout;

public class DeleteTimeoutRequest: IRequest<DeleteTimeoutResponse>
{
    public long PlugId { get; set; }
}