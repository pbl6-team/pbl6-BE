using AutoMapper;
using PBL6.Application.Contract.Examples.Dtos;
using PBL6.Application.Contract;
using PBL6.Domain.Models;
using PBL6.Domain.Models.Common;
using PBL6.Domain.Models.Users;
using PBL6.Application.Contract.Workspaces.Dtos;
using PBL6.Application.Contract.Channels.Dtos;

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
                    opt => opt.MapFrom(src => src.Members.Select(x => x.UserId))
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
        }
    }
}
