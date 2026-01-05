using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Commands.AddTimeout;

public class AddTimeoutRequestHandler: IRequestHandler<AddTimeoutRequest, AddTimeoutResponse>
{
    private readonly PlugsService _plugsService;

    public AddTimeoutRequestHandler(PlugsService plugsService)
    {
        _plugsService = plugsService;
    }

    public async Task<AddTimeoutResponse> Handle(AddTimeoutRequest request, CancellationToken cancellationToken)
    {
        return await _plugsService.AddTimeout(request, cancellationToken);
    }
}