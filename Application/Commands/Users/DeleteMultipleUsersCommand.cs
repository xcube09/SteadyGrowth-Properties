using MediatR;
using SteadyGrowth.Web.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Application.Commands.Users
{
    public class DeleteMultipleUsersCommand : IRequest<bool>
    {
        public List<string> UserIds { get; set; } = new List<string>();
    }

    public class DeleteMultipleUsersCommandHandler : IRequestHandler<DeleteMultipleUsersCommand, bool>
    {
        private readonly IMediator _mediator;

        public DeleteMultipleUsersCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteMultipleUsersCommand request, CancellationToken cancellationToken)
        {
            if (!request.UserIds.Any())
            {
                return false;
            }

            var deleteResults = new List<bool>();

            // Delete users one by one to ensure proper cascade handling
            foreach (var userId in request.UserIds)
            {
                var deleteCommand = new DeleteUserCommand { UserId = userId };
                var result = await _mediator.Send(deleteCommand, cancellationToken);
                deleteResults.Add(result);
            }

            // Return true if at least one deletion succeeded
            return deleteResults.Any(r => r);
        }
    }
}