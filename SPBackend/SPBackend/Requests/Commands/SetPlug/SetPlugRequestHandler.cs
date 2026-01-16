using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Commands.SetPlug;

public class SetPlugRequestHandler: IRequestHandler<SetPlugRequest, SetPlugResponse>
{
    private readonly PlugsService _plugService;

    public SetPlugRequestHandler(PlugsService plugService)
    {
        _plugService = plugService;
    }

    public async Task<SetPlugResponse> Handle(SetPlugRequest request, CancellationToken cancellationToken)
    {
        return await _plugService.SetPlug(request, cancellationToken);
    }
}