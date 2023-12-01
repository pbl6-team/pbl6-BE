using PBL6.Domain.Models.Users;

namespace PBL6.Domain.Data.Users
{
    public interface IMessageRepository : IRepository<Message>
    {
        Task<bool> CheckUserInConversation(Guid currentUserId, Guid id);
        Task<Message> Get(Guid id);
        Task<IEnumerable<Message>> GetMessagesOfChannelAsync(Guid value, DateTimeOffset timeCusor, int count);
        Task<IEnumerable<Message>> GetMessagesOfUserAsync(Guid currentUserId, Guid ToUserId, DateTimeOffset timeCusor, int count);
    }
}