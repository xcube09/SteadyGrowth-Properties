using SteadyGrowth.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Data;
using Microsoft.EntityFrameworkCore;
using MediatR;
using SteadyGrowth.Web.Application.Commands.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IMediator _mediator;
        
        public UserService(ApplicationDbContext db, ILogger<UserService> logger, IMediator mediator)
        {
            _db = db;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _db.Users.Update(user);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", user.Id);
                return false;
            }
        }

        public async Task<UserStats> GetUserStatsAsync(string userId)
        {
            var stats = new UserStats();
            var user = await _db.Users.Include(u => u.Properties).FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                stats.TotalProperties = user.Properties.Count;
                stats.ApprovedProperties = user.Properties.Count(p => p.Status == PropertyStatus.Approved);
                stats.PendingProperties = user.Properties.Count(p => p.Status == PropertyStatus.Pending);
                stats.TotalReferrals = await _db.Referrals.CountAsync(r => r.ReferrerId == userId);
                stats.TotalRewardPoints = await _db.Rewards.Where(r => r.UserId == userId).SumAsync(r => (int?)r.Points) ?? 0;
            }
            return stats;
        }

        public async Task<bool> DeactivateUserAsync(string userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;
            user.LockoutEnd = DateTime.UtcNow.AddYears(100);
            await _db.SaveChangesAsync();
            return true;
        }

        

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _db.Users.CountAsync();
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var command = new DeleteUserCommand { UserId = userId };
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> DeleteMultipleUsersAsync(List<string> userIds)
        {
            try
            {
                var command = new DeleteMultipleUsersCommand { UserIds = userIds };
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting multiple users: {UserIds}", string.Join(", ", userIds));
                return false;
            }
        }
    }
}
