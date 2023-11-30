using AutoMapper;
using PBL6.Application.Contract.Examples.Dtos;
using PBL6.Application.Contract;
using PBL6.Domain.Models;
using PBL6.Domain.Models.Common;
using PBL6.Domain.Models.Users;
using PBL6.Application.Contract.Workspaces.Dtos;
using PBL6.Application.Contract.Channels.Dtos;
using System.Data.Common;
using PBL6.Application.Contract.Users.Dtos;
using Application.Contract.Users.Dtos;

namespace PBL6.Application
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AuditedEntity, AuditedDto>();
            CreateMap<FullAuditedEntity, FullAuditedDto>().IncludeBase<AuditedEntity, AuditedDto>();

            CreateMap<Example, ExampleDto>().IncludeBase<FullAuditedEntity, FullAuditedDto>();
            CreateMap<CreateUpdateExampleDto, Example>();
            CreateMap<CreateUpdateExampleDto, ExampleDto>();

            CreateMap<Workspace, WorkspaceDto>()
                .IncludeBase<FullAuditedEntity, FullAuditedDto>()
                .ForMember(
                    x => x.Channels,
                    opt => opt.MapFrom(src => src.Channels.Select(x => x.Id))
                )
                .ForMember(
                    x => x.Members,
                    opt => opt.MapFrom(src => src.Members.Select(m => new UserDto
                    {
                        Id = m.UserId,
                        FirstName = m.User.Information.FirstName,
                        LastName = m.User.Information.LastName,
                        Picture = m.User.Information.Picture
                    }))
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
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id));
            CreateMap<CreateUpdatePermissionDto, PermissionsOfWorkspaceRole>()
                .ForMember(x => x.PermissionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.Id, opt => opt.Ignore());
            CreateMap<CreateUpdateWorkspaceRoleDto, WorkspaceRole>()
                .ForMember(x => x.Permissions, opt => opt.MapFrom(src => src.Permissions));
            CreateMap<PermissionsOfWorkspaceRole, PermissionDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Permission.Id))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Permission.Name))
                .ForMember(x => x.Code, opt => opt.MapFrom(src => src.Permission.Code))
                .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Permission.Description));
            CreateMap<WorkspacePermission, PermissionDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.IsEnabled, opt => opt.MapFrom(src => false));

            CreateMap<ChannelRole, ChannelRoleDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id));
            CreateMap<CreateUpdatePermissionDto, PermissionsOfChannelRole>()
                .ForMember(x => x.PermissionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.Id, opt => opt.Ignore());
            CreateMap<CreateUpdateChannelRoleDto, ChannelRole>()
                .ForMember(x => x.Permissions, opt => opt.MapFrom(src => src.Permissions));
            CreateMap<PermissionsOfChannelRole, PermissionDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Permission.Id))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Permission.Name))
                .ForMember(x => x.Code, opt => opt.MapFrom(src => src.Permission.Code))
                .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Permission.Description));
            CreateMap<ChannelPermission, PermissionDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.IsEnabled, opt => opt.MapFrom(src => false));

            CreateMap<User, UserDto2>()
                .ForMember(x => x.FirstName, opt => opt.MapFrom(src => src.Information.FirstName))
                .ForMember(x => x.LastName, opt => opt.MapFrom(src => src.Information.LastName))
                .ForMember(x => x.Gender, opt => opt.MapFrom(src => src.Information.Gender))
                .ForMember(x => x.Phone, opt => opt.MapFrom(src => src.Information.Phone))
                .ForMember(x => x.BirthDay, opt => opt.MapFrom(src => src.Information.BirthDay))
                .ForMember(x => x.Picture, opt => opt.MapFrom(src => src.Information.Picture));

            CreateMap<UpdateUserDto, User>()
                .ForPath(x => x.Information.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForPath(x => x.Information.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(x => x.Email, opt => opt.MapFrom(src => src.Email))
                .ForPath(x => x.Information.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForPath(x => x.Information.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForPath(x => x.Information.BirthDay, opt => opt.MapFrom(src => src.BirthDay));

        }
    }
}
