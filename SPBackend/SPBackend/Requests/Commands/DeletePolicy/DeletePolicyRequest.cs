using MediatR;

namespace SPBackend.Requests.Commands.DeletePolicy;

public class DeletePolicyRequest: IRequest<DeletePolicyResponse>
{
    public long PolicyId { get; set; }
}