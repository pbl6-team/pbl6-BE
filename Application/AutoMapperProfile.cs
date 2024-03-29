using AutoMapper;
using PBL6.Application.Contract;
using PBL6.Domain.Models.Common;
using PBL6.Domain.Models.Users;
using PBL6.Application.Contract.Workspaces.Dtos;
using PBL6.Application.Contract.Channels.Dtos;
using PBL6.Application.Contract.Users.Dtos;
using PBL6.Application.Contract.Chats.Dtos;
using Application.Contract.Workspaces.Dtos;
using PBL6.Application.Contract.Notifications.Dtos;
using PBL6.Common.Enum;
using Application.Contract.Users.Dtos;
using PBL6.Domain.Models.Admins;
using Application.Contract.Admins.Dtos;
using PBL6.Application.Contract.Meetings.Dtos;

namespace PBL6.Application
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AuditedEntity, AuditedDto>();
            CreateMap<FullAuditedEntity, FullAuditedDto>().IncludeBase<AuditedEntity, AuditedDto>();

            CreateMap<Workspace, WorkspaceDto>()
                .IncludeBase<FullAuditedEntity, FullAuditedDto>()
                .ForMember(
                    x => x.Channels,
                    opt => opt.MapFrom(src => src.Channels.Select(x => x.Id))
                )
                .ForMember(
                    x => x.Members,
                    opt =>
                        opt.MapFrom(
                            src =>
                                src.Members.Select(
                                    m =>
                                        new LessDetailWorkspaceUserDto
                                        {
                                            Id = m.UserId,
                                            FirstName = m.User.Information.FirstName,
                                            LastName = m.User.Information.LastName,
                                            Picture = m.User.Information.Picture,
                                            Status = m.Status
                                        }
                                )
                        )
                );
            CreateMap<Workspace, AdminWorkspaceDto>()
                .IncludeBase<FullAuditedEntity, FullAuditedDto>()
                .ForMember(
                    x => x.OwnerName,
                    opt =>
                        opt.MapFrom(
                            src =>
                                src.Owner.Information.FirstName
                                + " "
                                + src.Owner.Information.LastName
                        )
                );
            CreateMap<CreateWorkspaceDto, Workspace>();
            CreateMap<CreateWorkspaceDto, WorkspaceDto>();
            CreateMap<UpdateWorkspaceDto, Workspace>();
            CreateMap<UpdateWorkspaceDto, WorkspaceDto>();
            CreateMap<UpdateAvatarWorkspaceDto, Workspace>();
            CreateMap<UpdateAvatarWorkspaceDto, WorkspaceDto>();

            CreateMap<Channel, ChannelDto>()
                .IncludeBase<FullAuditedEntity, FullAuditedDto>()
                .ForMember(
                    x => x.ChannelMembers,
                    opt => opt.MapFrom(src => src.ChannelMembers.Select(x => x.UserId))
                );
            CreateMap<CreateChannelDto, Channel>();
            CreateMap<CreateChannelDto, ChannelDto>();
            CreateMap<UpdateChannelDto, Channel>();
            CreateMap<UpdateChannelDto, ChannelDto>();

            CreateMap<WorkspaceRole, WorkspaceRoleDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.NumberOfMembers, opt => opt.MapFrom(src => src.Members.Count));
            CreateMap<CreateUpdatePermissionDto, PermissionsOfWorkspaceRole>()
                .ForMember(x => x.PermissionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.Id, opt => opt.Ignore());
            CreateMap<CreateUpdateWorkspaceRoleDto, WorkspaceRole>()
                .ForMember(x => x.Permissions, opt => opt.MapFrom(src => src.Permissions));
            CreateMap<PermissionsOfWorkspaceRole, PermissionDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Permission.Id))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Permission.Name))
                .ForMember(x => x.Code, opt => opt.MapFrom(src => src.Permission.Code))
                .ForMember(
                    x => x.Description,
                    opt => opt.MapFrom(src => src.Permission.Description)
                );
            CreateMap<WorkspacePermission, PermissionDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.IsEnabled, opt => opt.MapFrom(src => false));

            CreateMap<ChannelRole, ChannelRoleDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.NumberOfMembers, opt => opt.MapFrom(src => src.Members.Count));
            CreateMap<CreateUpdatePermissionDto, PermissionsOfChannelRole>()
                .ForMember(x => x.PermissionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.Id, opt => opt.Ignore());
            CreateMap<CreateUpdateChannelRoleDto, ChannelRole>()
                .ForMember(x => x.Permissions, opt => opt.MapFrom(src => src.Permissions));
            CreateMap<PermissionsOfChannelRole, PermissionDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Permission.Id))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Permission.Name))
                .ForMember(x => x.Code, opt => opt.MapFrom(src => src.Permission.Code))
                .ForMember(
                    x => x.Description,
                    opt => opt.MapFrom(src => src.Permission.Description)
                );
            CreateMap<ChannelPermission, PermissionDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.IsEnabled, opt => opt.MapFrom(src => false));

            CreateMap<User, UserDetailDto>()
                .ForMember(x => x.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(x => x.FirstName, opt => opt.MapFrom(src => src.Information.FirstName))
                .ForMember(x => x.LastName, opt => opt.MapFrom(src => src.Information.LastName))
                .ForMember(x => x.Gender, opt => opt.MapFrom(src => src.Information.Gender))
                .ForMember(x => x.Phone, opt => opt.MapFrom(src => src.Information.Phone))
                .ForMember(x => x.BirthDay, opt => opt.MapFrom(src => src.Information.BirthDay))
                .ForMember(x => x.Picture, opt => opt.MapFrom(src => src.Information.Picture));

            CreateMap<User, UserNotInChannelDto>()
                .ForMember(x => x.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(x => x.FirstName, opt => opt.MapFrom(src => src.Information.FirstName))
                .ForMember(x => x.LastName, opt => opt.MapFrom(src => src.Information.LastName))
                .ForMember(x => x.Gender, opt => opt.MapFrom(src => src.Information.Gender))
                .ForMember(x => x.Phone, opt => opt.MapFrom(src => src.Information.Phone))
                .ForMember(x => x.BirthDay, opt => opt.MapFrom(src => src.Information.BirthDay))
                .ForMember(x => x.Picture, opt => opt.MapFrom(src => src.Information.Picture));

            CreateMap<User, AdminUserDto>()
                .ForMember(x => x.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(x => x.FirstName, opt => opt.MapFrom(src => src.Information.FirstName))
                .ForMember(x => x.LastName, opt => opt.MapFrom(src => src.Information.LastName))
                .ForMember(x => x.Gender, opt => opt.MapFrom(src => src.Information.Gender))
                .ForMember(x => x.Phone, opt => opt.MapFrom(src => src.Information.Phone))
                .ForMember(x => x.BirthDay, opt => opt.MapFrom(src => src.Information.BirthDay))
                .ForMember(x => x.Picture, opt => opt.MapFrom(src => src.Information.Picture))
                .ForMember(x => x.Status, opt => opt.MapFrom(src => src.Information.Status));

            CreateMap<UpdateUserDto, User>()
                .ForPath(x => x.Information.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForPath(x => x.Information.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(x => x.Email, opt => opt.MapFrom(src => src.Email))
                .ForPath(x => x.Information.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForPath(x => x.Information.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForPath(x => x.Information.BirthDay, opt => opt.MapFrom(src => src.BirthDay));

            CreateMap<Meeting, MeetingDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<Message, MessageDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.SendAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(x => x.IsChannel, opt => opt.MapFrom(src => src.ToChannelId != null))
                .ForMember(
                    x => x.ChildCount,
                    opt => opt.MapFrom(src => src.Children.Where(x => !x.IsDeleted).Count())
                )
                .ForMember(
                    x => x.ReceiverId,
                    opt => opt.MapFrom(src => src.ToChannelId ?? src.ToUserId)
                )
                .ForMember(
                    x => x.SenderAvatar,
                    opt => opt.MapFrom(src => src.Sender.Information.Picture)
                )
                .ForMember(
                    x => x.SenderName,
                    opt =>
                        opt.MapFrom(
                            src =>
                                src.Sender.Information.FirstName
                                + " "
                                + src.Sender.Information.LastName
                        )
                )
                .ForMember(
                    x => x.Reaction,
                    opt =>
                        opt.MapFrom(
                            src => string.Join(",", src.MessageTrackings.Select(x => x.Reaction))
                        )
                )
                .ForMember(
                    x => x.Readers,
                    opt =>
                        opt.MapFrom(
                            src =>
                                src.MessageTrackings
                                    .Where(x => x.ReadTime != null && x.IsDeleted)
                                    .Select(
                                        x =>
                                            new Reader
                                            {
                                                Id = x.UserId,
                                                Name =
                                                    x.User.Information.FirstName
                                                    + " "
                                                    + x.User.Information.LastName,
                                                Avatar = x.User.Information.Picture,
                                                ReadTime = x.ReadTime.Value
                                            }
                                    )
                        )
                )
                .ForMember(
                    x => x.Files,
                    opt => opt.MapFrom(src => src.Files.Where(x => !x.IsDeleted))
                );

            CreateMap<WorkspaceMember, WorkspaceUserDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(x => x.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(x => x.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(
                    x => x.FirstName,
                    opt => opt.MapFrom(src => src.User.Information.FirstName)
                )
                .ForMember(
                    x => x.LastName,
                    opt => opt.MapFrom(src => src.User.Information.LastName)
                )
                .ForMember(x => x.Gender, opt => opt.MapFrom(src => src.User.Information.Gender))
                .ForMember(x => x.Phone, opt => opt.MapFrom(src => src.User.Information.Phone))
                .ForMember(
                    x => x.BirthDay,
                    opt => opt.MapFrom(src => src.User.Information.BirthDay)
                )
                .ForMember(x => x.Picture, opt => opt.MapFrom(src => src.User.Information.Picture))
                .ForMember(x => x.Role, opt => opt.MapFrom(src => src.WorkspaceRole))
                .ForMember(x => x.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(
                    x => x.IsOwner,
                    opt => opt.MapFrom(src => src.Workspace.OwnerId == src.UserId)
                );

            CreateMap<ChannelMember, ChannelUserDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(x => x.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(
                    x => x.FirstName,
                    opt => opt.MapFrom(src => src.User.Information.FirstName)
                )
                .ForMember(
                    x => x.LastName,
                    opt => opt.MapFrom(src => src.User.Information.LastName)
                )
                .ForMember(x => x.Gender, opt => opt.MapFrom(src => src.User.Information.Gender))
                .ForMember(x => x.Phone, opt => opt.MapFrom(src => src.User.Information.Phone))
                .ForMember(
                    x => x.BirthDay,
                    opt => opt.MapFrom(src => src.User.Information.BirthDay)
                )
                .ForMember(x => x.Picture, opt => opt.MapFrom(src => src.User.Information.Picture))
                .ForMember(x => x.Role, opt => opt.MapFrom(src => src.ChannelRole))
                .ForMember(x => x.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(
                    x => x.IsOwner,
                    opt => opt.MapFrom(src => src.Channel.OwnerId == src.UserId)
                );

            CreateMap<Notification, NotificationDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(
                    x => x.IsRead,
                    opt =>
                        opt.MapFrom(
                            src =>
                                src.UserNotifications.First().Status
                                == ((short)NOTIFICATION_STATUS.READ)
                        )
                )
                .ForMember(x => x.Data, opt => opt.MapFrom(src => src.Data));

            CreateMap<FileOfMessage, FileInfoDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(x => x.Url, opt => opt.MapFrom(src => src.Url));

            CreateMap<AdminAccount, AdminDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.FirstName, opt => opt.MapFrom(src => src.Information.FirstName))
                .ForMember(x => x.LastName, opt => opt.MapFrom(src => src.Information.LastName))
                .ForMember(x => x.Phone, opt => opt.MapFrom(src => src.Information.Phone))
                .ForMember(x => x.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(x => x.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(x => x.Status, opt => opt.MapFrom(src => src.Information.Status));
            CreateMap<AdminAccount, AdminDetailDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.FirstName, opt => opt.MapFrom(src => src.Information.FirstName))
                .ForMember(x => x.LastName, opt => opt.MapFrom(src => src.Information.LastName))
                .ForMember(x => x.Phone, opt => opt.MapFrom(src => src.Information.Phone))
                .ForMember(x => x.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(x => x.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(x => x.Status, opt => opt.MapFrom(src => src.Information.Status))
                .ForMember(x => x.Gender, opt => opt.MapFrom(src => src.Information.Gender))
                .ForMember(x => x.BirthDay, opt => opt.MapFrom(src => src.Information.BirthDate));
            CreateMap<UpdateAdminDto, AdminAccount>()
                .ForPath(x => x.Information.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForPath(x => x.Information.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(x => x.Email, opt => opt.MapFrom(src => src.Email))
                .ForPath(x => x.Information.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForPath(x => x.Information.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForPath(x => x.Information.BirthDate, opt => opt.MapFrom(src => src.BirthDay));

            CreateMap<Meeting, MeetingInfo>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(
                    x => x.ChannelName,
                    opt => opt.MapFrom(src => src.Channel != null ? src.Channel.Name : "")
                )
                .ForMember(
                    x => x.WorkspaceName,
                    opt =>
                        opt.MapFrom(
                            src =>
                                (src.Channel != null && src.Channel.Workspace != null)
                                    ? src.Channel.Workspace.Name
                                    : ""
                        )
                )
                .ForMember(
                    x => x.Participants,
                    opt =>
                        opt.MapFrom(
                            src =>
                                src.Channel.ChannelMembers.Where(
                                    x => x.Status == (short)CHANNEL_MEMBER_STATUS.ACTIVE
                                )
                        )
                )
                .ForMember(
                    opt => opt.WorkspaceId,
                    opt => opt.MapFrom(src => src.Channel.WorkspaceId)
                );
        }
    }
}
