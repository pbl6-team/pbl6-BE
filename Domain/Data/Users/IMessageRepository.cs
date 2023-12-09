using PBL6.Domain.Models.Users;

namespace PBL6.Domain.Data.Users
{
    public interface IMessageRepository : IRepository<Message>
    {
        Task<bool> CheckUserInConversation(Guid currentUserId, Guid id);
        Task<Message> Get(Guid id);
        IQueryable<Message> GetConversations(Guid currentUserId, string search);
        Task<IEnumerable<Message>> GetMessagesOfChannelAsync(Guid channelId, DateTimeOffset timeCusor, int count, Guid currentUserId);
        Task<IEnumerable<Message>> GetMessagesOfUserAsync(Guid currentUserId, Guid ToUserId, DateTimeOffset timeCursor, int count);
    }
}