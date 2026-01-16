using MediatR;

namespace SPBackend.Requests.Commands.SetPlug;

public class SetPlugRequest: IRequest<SetPlugResponse>
{
    public long PlugId { get; set; }
    public bool SwitchOn { get; set; }
}