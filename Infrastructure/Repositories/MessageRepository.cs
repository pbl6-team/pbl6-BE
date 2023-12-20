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
                // || currentUserId == message.ToChannel.Workspace.CreatedBy
                || message.ToChannel.ChannelMembers.Any(
                    x => x.UserId == currentUserId && !x.IsDeleted
                )
            )
            {
                return true;
            }

            return false;
        }

        public async Task<int> CountUnreadMessage(Guid currentUserId)
        {
            return await _dbSet
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
                        && !x.IsDeleted
                )
                .OrderByDescending(x => x.CreatedAt)
                .GroupBy(x => currentUserId == x.CreatedBy ? x.ToUserId : x.CreatedBy)
                .OrderByDescending(x => x.First().CreatedAt)
                .CountAsync(
                    x =>
                        !(x.OrderByDescending(x => x.CreatedAt).First().CreatedBy == currentUserId
                        || x.OrderByDescending(x => x.CreatedAt)
                            .First()
                            .MessageTrackings.Any(
                                x => x.UserId == currentUserId && !x.IsDeleted && x.IsRead
                            ))
                );
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

        public async Task<Message> GetMessageByFileIds(IEnumerable<Guid> fileIds)
        {
            return await _apiDbContext.Messages
                .Include(x => x.Files)
                .FirstOrDefaultAsync(x => x.Files.Any(x => fileIds.Contains(x.Id)));
        }

        public async Task<IEnumerable<Message>> GetMessagesOfChannelAsync(
            Guid currentUserId,
            Guid channelId,
            Guid? parentId,
            DateTimeOffset timeCursor,
            int count,
            bool isBefore = true
        )
        {
            var message = _apiDbContext.Messages
                .Include(x => x.MessageTrackings)
                .ThenInclude(x => x.User)
                .ThenInclude(x => x.Information)
                .Include(x => x.Sender)
                .ThenInclude(x => x.Information)
                .Include(x => x.Files)
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
                        && (isBefore ? x.CreatedAt < timeCursor : x.CreatedAt > timeCursor)
                        && !x.IsDeleted
                        && !x.MessageTrackings.Any(x => x.UserId == currentUserId && x.IsDeleted)
                        && x.ParentId == parentId
                );

            if (isBefore)
            {
                return await message.OrderByDescending(x => x.CreatedAt).Take(count).ToListAsync();
            }
            else
            {
                return await message
                    .OrderBy(x => x.CreatedAt)
                    .Take(count)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<Message>> GetMessagesOfUserAsync(
            Guid currentUserId,
            Guid toUserId,
            Guid? parentId,
            DateTimeOffset timeCursor,
            int count,
            bool isBefore = true
        )
        {
            var message = _apiDbContext.Messages
                .Include(x => x.MessageTrackings)
                .ThenInclude(x => x.User)
                .ThenInclude(x => x.Information)
                .Include(x => x.Sender)
                .ThenInclude(x => x.Information)
                .Include(x => x.Files)
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
                        && (isBefore ? x.CreatedAt < timeCursor : x.CreatedAt > timeCursor)
                        && !x.IsDeleted
                        && x.ToChannelId == null
                        && x.ParentId == parentId
                        && !x.MessageTrackings.Any(x => x.UserId == currentUserId && x.IsDeleted)
                );

            if (isBefore)
            {
                return await message.OrderByDescending(x => x.CreatedAt).Take(count).ToListAsync();
            }
            else
            {
                return await message
                    .OrderBy(x => x.CreatedAt)
                    .Take(count)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<Message>> GetPinMessagesOfChannelAsync(
            Guid currentUserId,
            Guid channelId,
            int offset,
            int limit
        )
        {
            return await _apiDbContext.Messages
                .Include(x => x.MessageTrackings)
                .ThenInclude(x => x.User)
                .ThenInclude(x => x.Information)
                .Include(x => x.Sender)
                .ThenInclude(x => x.Information)
                .Include(x => x.Files)
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
                        && x.IsPined
                        && !x.IsDeleted
                        && !x.MessageTrackings.Any(x => x.UserId == currentUserId && x.IsDeleted)
                )
                .OrderByDescending(x => x.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
            ;
        }

        public async Task<IEnumerable<Message>> GetPinMessagesOfUserAsync(
            Guid currentUserId,
            Guid UserId,
            int offset,
            int limit
        )
        {
            return await _apiDbContext.Messages
                .Include(x => x.MessageTrackings)
                .ThenInclude(x => x.User)
                .ThenInclude(x => x.Information)
                .Include(x => x.Sender)
                .ThenInclude(x => x.Information)
                .Include(x => x.Files)
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
                            (x.ToUserId == currentUserId && x.CreatedBy == UserId)
                            || (x.ToUserId == UserId && x.CreatedBy == currentUserId)
                        )
                        && x.IsPined
                        && !x.IsDeleted
                        && x.ToChannelId == null
                        && !x.MessageTrackings.Any(x => x.UserId == currentUserId && x.IsDeleted)
                )
                .OrderByDescending(x => x.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }
    }
}
