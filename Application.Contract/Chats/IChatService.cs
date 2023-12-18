using PBL6.Application.Contract.Chats.Dtos;

namespace PBL6.Application.Contract.Chats
{
    public interface IChatService
    {
        Task<List<MessageDto>> GetMessagesAsync(GetMessageDto input);  
        Task<MessageDto> SendMessageAsync(SendMessageDto input);
        Task<MessageDto> UpdateMessageAsync(UpdateMessageDto input);
        Task<MessageDto> DeleteMessageAsync(Guid id, bool isDeleteEverywhere = false);
        Task<List<ConversationDto>> GetConversationsAsync(ConversationRequest input);
        Task<MessageDto> ReactMessageAsync(ReactMessageDto input);
        Task<MessageDto> DeleteFile (IEnumerable<Guid> ids);
        Task<MessageDto> ReadMessageAsync(Guid messageId);
        Task<IEnumerable<FileInfoDto>> GetFilesAsync(GetFileDto input);
    }
}