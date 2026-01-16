using MediatR;
using SPBackend.Services.Mains;

namespace SPBackend.Requests.Queries.GetPowerSource;

public class GetPowerSourceRequestHandler: IRequestHandler<GetPowerSourceRequest, GetPowerSourceResponse>
{
    private readonly PowerSourceService _powerSourceService;

    public GetPowerSourceRequestHandler(PowerSourceService powerSourceService)
    {
        _powerSourceService = powerSourceService;
    }

    public Task<GetPowerSourceResponse> Handle(GetPowerSourceRequest request, CancellationToken cancellationToken)
    {
        return _powerSourceService.GetPowerSource();
    }
}