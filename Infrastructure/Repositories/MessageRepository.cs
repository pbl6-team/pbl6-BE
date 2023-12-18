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
                || message.ToChannel.ChannelMembers.Any(
                    x => x.UserId == currentUserId && !x.IsDeleted
                )
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

        public IQueryable<Message> GetConversations(Guid currentUserId, string search)
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
                        (x.ToUserId == currentUserId || x.CreatedBy == currentUserId)
                        && x.ToChannelId == null
                        && (
                            currentUserId == x.CreatedBy
                                ? (
                                    (
                                        x.Receiver.Information.FirstName
                                        + " "
                                        + x.Receiver.Information.LastName
                                    )
                                        .ToLower()
                                        .Contains(search.ToLower())
                                    || x.Receiver.Email.ToLower().Contains(search.ToLower())
                                )
                                : (
                                    (
                                        x.Sender.Information.FirstName
                                        + " "
                                        + x.Sender.Information.LastName
                                    )
                                        .ToLower()
                                        .Contains(search.ToLower())
                                    || x.Sender.Email.ToLower().Contains(search.ToLower())
                                )
                        )
                        && !x.IsDeleted
                );

            return result;
        }

        public async Task<Message> GetMessageByFileId(Guid fileId)
        {
            return (
                await _apiDbContext.Files
                    .Include(x => x.Message)
                    .ThenInclude(x => x.Files)
                    .FirstOrDefaultAsync(x => x.Id == fileId)
            )?.Message;
        }

        public async Task<IEnumerable<Message>> GetMessagesOfChannelAsync(
            Guid currentUserId,
            Guid channelId,
            Guid? parentId,
            DateTimeOffset timeCursor,
            int count
        )
        {
            return await _apiDbContext.Messages
                .Include(x => x.MessageTrackings)
                .ThenInclude(x => x.User)
                .ThenInclude(x => x.Information)
                .Include(x => x.Sender)
                .ThenInclude(x => x.Information)
                .Include(
                    x =>
                        x.Children.Where(
                            x =>
                                !x.IsDeleted
                                && !x.MessageTrackings.Any(
                                    x => x.UserId == currentUserId && !x.IsDeleted
                                )
                        )
                )
                .ThenInclude(x => x.MessageTrackings)
                .AsNoTracking()
                .Where(
                    x =>
                        x.ToChannelId == channelId
                        && x.CreatedAt < timeCursor
                        && !x.IsDeleted
                        && !x.MessageTrackings.Any(x => x.UserId == currentUserId && x.IsDeleted)
                        && x.ParentId == parentId
                )
                .OrderByDescending(x => x.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetMessagesOfUserAsync(
            Guid currentUserId,
            Guid toUserId,
            Guid? parentId,
            DateTimeOffset timeCursor,
            int count
        )
        {
            return await _apiDbContext.Messages
                .Include(x => x.MessageTrackings)
                .ThenInclude(x => x.User)
                .ThenInclude(x => x.Information)
                .Include(x => x.Sender)
                .ThenInclude(x => x.Information)
                .Include(
                    x =>
                        x.Children.Where(
                            x =>
                                !x.IsDeleted
                                && !x.MessageTrackings.Any(
                                    x => x.UserId == currentUserId && !x.IsDeleted
                                )
                        )
                )
                .ThenInclude(x => x.MessageTrackings)
                .AsNoTracking()
                .Where(
                    x =>
                        (
                            (x.ToUserId == currentUserId && x.CreatedBy == toUserId)
                            || (x.ToUserId == toUserId && x.CreatedBy == currentUserId)
                        )
                        && x.CreatedAt < timeCursor
                        && !x.IsDeleted
                        && x.ToChannelId == null
                        && x.ParentId == parentId
                        && !x.MessageTrackings.Any(x => x.UserId == currentUserId && x.IsDeleted)
                )
                .OrderByDescending(x => x.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}
