using SteadyGrowth.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Services.Implementations
{
    public class RewardService : IRewardService
    {
        private readonly ILogger<RewardService> _logger;
        public RewardService(ILogger<RewardService> logger)
        {
            _logger = logger;
        }

        public Task<bool> AddRewardPointsAsync(string userId, int points, string description, RewardType rewardType) => Task.FromResult(false);
        public Task<bool> RedeemPointsAsync(string userId, int points, decimal moneyValue) => Task.FromResult(false);
        public Task<IEnumerable<Reward>> GetUserRewardsAsync(string userId) => Task.FromResult<IEnumerable<Reward>>(Array.Empty<Reward>());
        public Task<decimal> CalculateRewardValueAsync(int points) => Task.FromResult(0m);
        public Task<int> GetUserTotalPointsAsync(string userId) => Task.FromResult(0);
    }
}
