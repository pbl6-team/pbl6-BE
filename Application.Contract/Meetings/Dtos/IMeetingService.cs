namespace PBL6.Application.Contract.Meetings.Dtos
{
    public interface IMeetingService
    {
        Task<MeetingDto> CreateMeetingAsync(CreateMeetingDto input);
        Task<MeetingDto> UpdateMeetingAsync(Guid id, UpdateMeetingDto input);
        Task DeleteMeetingAsync(Guid id);
        Task<string> JoinMeetingAsync(JoinMeetingDto input);
        Task<string> MakeCallAsync(MakeCallDto input);
        Task EndCallAsync(JoinMeetingDto joinMeetingDto);
        Task<string> JoinCallAsync(JoinMeetingDto joinMeetingDto);
    }
}
