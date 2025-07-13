using MediatR;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Application.Commands.UpgradeRequests
{
    public class CreateUpgradeRequestCommand : IRequest<UpgradeRequest>
    {
        
        public string? UserId { get; set; }

        [Required]
        public int RequestedPackageId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [StringLength(500)]
        public string? PaymentDetails { get; set; }
    }

    public class CreateUpgradeRequestCommandHandler : IRequestHandler<CreateUpgradeRequestCommand, UpgradeRequest>
    {
        private readonly ApplicationDbContext _context;

        public CreateUpgradeRequestCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UpgradeRequest> Handle(CreateUpgradeRequestCommand request, CancellationToken cancellationToken)
        {
            var upgradeRequest = new UpgradeRequest
            {
                UserId = request.UserId,
                RequestedPackageId = request.RequestedPackageId,
                PaymentMethod = request.PaymentMethod,
                PaymentDetails = request.PaymentDetails,
                Status = UpgradeRequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            _context.UpgradeRequests.Add(upgradeRequest);
            await _context.SaveChangesAsync(cancellationToken);

            return upgradeRequest;
        }
    }
}
