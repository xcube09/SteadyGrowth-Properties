using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SteadyGrowth.Web.Application.Commands.UpgradeRequests;
using SteadyGrowth.Web.Application.Queries.UpgradeRequests;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Services.Implementations
{
    /// <summary>
    /// Service implementation for managing academy upgrade requests.
    /// </summary>
    public class UpgradeRequestService : IUpgradeRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMediator _mediator;
        private readonly ILogger<UpgradeRequestService> _logger;

        public UpgradeRequestService(
            ApplicationDbContext context,
            IMediator mediator,
            ILogger<UpgradeRequestService> logger)
        {
            _context = context;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<int> GetPendingUpgradeRequestsCountAsync()
        {
            try
            {
                return await _context.UpgradeRequests
                    .CountAsync(ur => ur.Status == UpgradeRequestStatus.Pending);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending upgrade requests count");
                return 0;
            }
        }

        public async Task<IList<UpgradeRequest>> GetPendingUpgradeRequestsAsync()
        {
            try
            {
                var query = new GetPendingUpgradeRequestsQuery();
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending upgrade requests");
                return new List<UpgradeRequest>();
            }
        }

        public async Task<IList<UpgradeRequest>> GetUpgradeRequestsAsync(
            UpgradeRequestStatus? status = null,
            string? searchTerm = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageIndex = 1,
            int pageSize = 20)
        {
            try
            {
                var query = new GetUpgradeRequestsQuery
                {
                    Status = status,
                    SearchTerm = searchTerm,
                    FromDate = fromDate,
                    ToDate = toDate,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upgrade requests");
                return new List<UpgradeRequest>();
            }
        }

        public async Task<UpgradeRequest?> GetUpgradeRequestByIdAsync(int id)
        {
            try
            {
                var query = new GetUpgradeRequestByIdQuery { Id = id };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upgrade request {RequestId}", id);
                return null;
            }
        }

        public async Task<bool> ApproveUpgradeRequestAsync(int requestId, string adminUserId)
        {
            try
            {
                var command = new ApproveUpgradeRequestCommand
                {
                    RequestId = requestId,
                    AdminUserId = adminUserId
                };
                var result = await _mediator.Send(command);
                
                if (result)
                {
                    _logger.LogInformation("Upgrade request {RequestId} approved by admin {AdminUserId}", requestId, adminUserId);
                }
                else
                {
                    _logger.LogWarning("Failed to approve upgrade request {RequestId}", requestId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving upgrade request {RequestId}", requestId);
                return false;
            }
        }

        public async Task<bool> RejectUpgradeRequestAsync(int requestId, string adminUserId, string? rejectionReason = null)
        {
            try
            {
                var command = new RejectUpgradeRequestCommand
                {
                    RequestId = requestId,
                    AdminUserId = adminUserId,
                    RejectionReason = rejectionReason
                };
                var result = await _mediator.Send(command);
                
                if (result)
                {
                    _logger.LogInformation("Upgrade request {RequestId} rejected by admin {AdminUserId}. Reason: {Reason}", 
                        requestId, adminUserId, rejectionReason ?? "None provided");
                }
                else
                {
                    _logger.LogWarning("Failed to reject upgrade request {RequestId}", requestId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting upgrade request {RequestId}", requestId);
                return false;
            }
        }

        public async Task<UpgradeRequestStats> GetUpgradeRequestStatsAsync()
        {
            try
            {
                var totalRequests = await _context.UpgradeRequests.CountAsync();
                var pendingRequests = await _context.UpgradeRequests
                    .CountAsync(ur => ur.Status == UpgradeRequestStatus.Pending);
                var approvedRequests = await _context.UpgradeRequests
                    .CountAsync(ur => ur.Status == UpgradeRequestStatus.Approved);
                var rejectedRequests = await _context.UpgradeRequests
                    .CountAsync(ur => ur.Status == UpgradeRequestStatus.Rejected);

                return new UpgradeRequestStats
                {
                    TotalRequests = totalRequests,
                    PendingRequests = pendingRequests,
                    ApprovedRequests = approvedRequests,
                    RejectedRequests = rejectedRequests
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upgrade request statistics");
                return new UpgradeRequestStats();
            }
        }
    }
}