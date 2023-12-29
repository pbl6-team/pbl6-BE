namespace PBL6.Application.Contract.Meetings.Dtos
{
    public interface IMeetingService
    {
        Task<MeetingDto> CreateMeetingAsync(CreateMeetingDto input);
        Task<MeetingDto> UpdateMeetingAsync(Guid id, UpdateMeetingDto input);
        Task<List<MeetingInfo>> GetMeetingsAsync();
        Task<MeetingInfo> GetMeetingAsync(Guid id);
        Task<List<MeetingInfo>> GetMeetingsByChannelIdAsync(Guid channelId);
        Task DeleteMeetingAsync(Guid id);
        Task<string> JoinMeetingAsync(JoinMeetingDto input);
        Task EndMeetingAsync(JoinMeetingDto input);
        Task<CallInfoDto> MakeCallAsync(MakeCallDto input);
        Task EndCallAsync(JoinMeetingDto joinMeetingDto);
        Task<CallInfoDto> JoinCallAsync(JoinMeetingDto joinMeetingDto);
    }
}
