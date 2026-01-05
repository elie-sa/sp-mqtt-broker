using MediatR;
using SPBackend.Commands.SetPlug;

namespace SPBackend.Commands.SetPlugName;

public class SetPlugNameRequest: IRequest<SetPlugNameResponse>
{
    public long PlugId { get; set; }
    public string Name { get; set; }
}