using MediatR;
using SPBackend.Services.Plugs;

namespace SPBackend.Requests.Commands.SetPlugName;

public class SetPlugNameRequestHandler: IRequestHandler<SetPlugNameRequest, SetPlugNameResponse>
{
    private readonly PlugsService _plugService;

    public SetPlugNameRequestHandler(PlugsService plugService)
    {
        _plugService = plugService;
    }

    public async Task<SetPlugNameResponse> Handle(SetPlugNameRequest request, CancellationToken cancellationToken)
    {
        return await _plugService.SetPlugName(request, cancellationToken);
    }
}