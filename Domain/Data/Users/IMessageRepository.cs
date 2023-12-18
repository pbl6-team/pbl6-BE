using PBL6.Domain.Models.Users;

namespace PBL6.Domain.Data.Users
{
    public interface IMessageRepository : IRepository<Message>
    {
        Task<bool> CheckUserInConversation(Guid currentUserId, Guid id);
        Task<Message> Get(Guid id);
        IQueryable<Message> GetConversations(Guid currentUserId, string search);
        Task<IEnumerable<Message>> GetMessagesOfChannelAsync(Guid currentUserId, Guid channelId, Guid? parentId, DateTimeOffset timeCursor, int count);
        Task<IEnumerable<Message>> GetMessagesOfUserAsync(Guid currentUserId, Guid toUserId, Guid? parentId, DateTimeOffset timeCursor, int count);
        Task<Message> GetMessageByFileId(Guid fileId);
    }
}