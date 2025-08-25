using MediatR;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Application.Commands.Academy
{
    public class UpdatePackagePriceCommand : IRequest<bool>
    {
        public int PackageId { get; set; }
        public decimal NewPrice { get; set; }
    }

    public class UpdatePackagePriceCommandHandler : IRequestHandler<UpdatePackagePriceCommand, bool>
    {
        private readonly IAcademyPackageService _academyPackageService;

        public UpdatePackagePriceCommandHandler(IAcademyPackageService academyPackageService)
        {
            _academyPackageService = academyPackageService;
        }

        public async Task<bool> Handle(UpdatePackagePriceCommand request, CancellationToken cancellationToken)
        {
            if (request.NewPrice < 0)
                return false;

            return await _academyPackageService.UpdatePackagePriceAsync(request.PackageId, request.NewPrice);
        }
    }
}