using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Commands.DeleteTimeout;

public class DeleteTimeoutRequestHandler: IRequestHandler<DeleteTimeoutRequest, DeleteTimeoutResponse>
{
    private readonly PlugsService _plugService;

    public DeleteTimeoutRequestHandler(PlugsService plugService)
    {
        _plugService = plugService;
    }

    public async Task<DeleteTimeoutResponse> Handle(DeleteTimeoutRequest request, CancellationToken cancellationToken)
    {
        return await _plugService.DeleteTimeout(request.PlugId, cancellationToken);
    }
}