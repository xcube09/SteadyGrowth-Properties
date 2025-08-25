using MediatR;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Application.Commands.Academy
{
    public class TogglePackageStatusCommand : IRequest<bool>
    {
        public int PackageId { get; set; }
    }

    public class TogglePackageStatusCommandHandler : IRequestHandler<TogglePackageStatusCommand, bool>
    {
        private readonly IAcademyPackageService _academyPackageService;

        public TogglePackageStatusCommandHandler(IAcademyPackageService academyPackageService)
        {
            _academyPackageService = academyPackageService;
        }

        public async Task<bool> Handle(TogglePackageStatusCommand request, CancellationToken cancellationToken)
        {
            return await _academyPackageService.TogglePackageActiveStatusAsync(request.PackageId);
        }
    }
}