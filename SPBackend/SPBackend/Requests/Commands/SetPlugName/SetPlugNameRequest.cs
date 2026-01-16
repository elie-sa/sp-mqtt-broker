using MediatR;

namespace SPBackend.Requests.Commands.SetPlugName;

public class SetPlugNameRequest: IRequest<SetPlugNameResponse>
{
    public long PlugId { get; set; }
    public string Name { get; set; }
}