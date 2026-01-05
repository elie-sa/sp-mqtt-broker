using MediatR;

namespace SPBackend.Commands.SetPlug;

public class SetPlugRequest: IRequest<SetPlugResponse>
{
    public long PlugId { get; set; }
    public bool SwitchOn { get; set; }
}