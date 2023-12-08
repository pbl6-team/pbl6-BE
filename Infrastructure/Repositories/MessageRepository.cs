using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Domain.Data.Users;
using PBL6.Domain.Models.Users;
using PBL6.Infrastructure.Data;

namespace PBL6.Infrastructure.Repositories
{
    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        public MessageRepository(ApiDbContext dbContext, ILogger logger)
            : base(dbContext, logger) { }

        public async Task<bool> CheckUserInConversation(Guid currentUserId, Guid id)
        {
            var message = await Get(id);
            if (
                //todo: check status of user in channel
                currentUserId == message.CreatedBy
                || currentUserId == message.ToUserId
                || currentUserId == message.ToChannel.CreatedBy
                || currentUserId == message.ToChannel.Workspace.CreatedBy
                || message.ToChannel.ChannelMembers.Any(x => x.UserId == currentUserId && !x.IsDeleted)
            )
            {
                return true;
            }

            return false;
        }

        public async Task<Message> Get(Guid id)
        {
            return await _apiDbContext.Messages
                .Include(x => x.Sender)
                .ThenInclude(x => x.Information)
                .Include(x => x.Children)
                .ThenInclude(x => x.Sender)
                .ThenInclude(x => x.Information)
                .Include(x => x.ToChannel)
                .ThenInclude(x => x.ChannelMembers)
                .Include(x => x.MessageTrackings)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public  IQueryable<Message> GetConversations(Guid currentUserId, string search)
        {
            var result = _dbSet
                .Include(x => x.Sender)
                .ThenInclude(x => x.Information)
                .Include(x => x.Receiver)
                .ThenInclude(x => x.Information)
                .Include(x => x.MessageTrackings)
                .ThenInclude(x => x.User)
                .ThenInclude(x => x.Information)
                .AsNoTracking()
                .Where(
                    x =>
                        (
                            x.ToUserId == currentUserId
                            || x.CreatedBy == currentUserId
                        )
                        && x.ToChannelId == null
                        && (
                            currentUserId == x.CreatedBy ?
                            (x.Receiver.Information.FirstName + " " + x.Receiver.Information.LastName).ToLower().Contains(search.ToLower())
                            : (x.Sender.Information.FirstName + " " + x.Sender.Information.LastName).ToLower().Contains(search.ToLower())
                        )
                        && !x.IsDeleted
                );
                
            return result;
        }

        public async Task<IEnumerable<Message>> GetMessagesOfChannelAsync(
            Guid value,
            DateTimeOffset timeCusor,
            int count
        )
        {
            return await _apiDbContext.Messages
                .Include(x => x.MessageTrackings)
                .ThenInclude(x => x.User)
                .ThenInclude(x => x.Information)
                .Include(x => x.Sender)
                .ThenInclude(x => x.Information)
                .Where(x => x.ToChannelId == value && x.CreatedAt < timeCusor && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetMessagesOfUserAsync(
            Guid currentUserId,
            Guid ToUserId,
            DateTimeOffset timeCusor,
            int count
        )
        {
            return await _apiDbContext.Messages
                .Include(x => x.MessageTrackings)
                .ThenInclude(x => x.User)
                .ThenInclude(x => x.Information)
                .Include(x => x.Sender)
                .ThenInclude(x => x.Information)
                .Where(
                    x =>
                        (
                            (x.ToUserId == currentUserId && x.CreatedBy == ToUserId)
                            || (x.ToUserId == ToUserId && x.CreatedBy == currentUserId)
                        )
                        && x.CreatedAt < timeCusor
                        && !x.IsDeleted
                )
                .OrderByDescending(x => x.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}
