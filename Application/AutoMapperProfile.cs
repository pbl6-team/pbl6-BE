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
            CreateMap<FullAuditedEntity, FullAuditedDto>()
                .IncludeBase<AuditedEntity, AuditedDto>();

            CreateMap<Example, ExampleDto>()
                .IncludeBase<FullAuditedEntity, FullAuditedDto>();
            CreateMap<CreateUpdateExampleDto, Example>();
            CreateMap<CreateUpdateExampleDto, ExampleDto>();
            
            CreateMap<Workspace, WorkspaceDto>()
                .IncludeBase<FullAuditedEntity, FullAuditedDto>()
                .ForMember(x => x.Channels, opt => opt.MapFrom(src => src.Channels.Select(x => x.Id)))
                .ForMember(x => x.Members, opt => opt.MapFrom(src => src.Members.Select(x => x.UserId)));
            CreateMap<CreateWorkspaceDto, Workspace>();
            CreateMap<CreateWorkspaceDto, WorkspaceDto>();
            CreateMap<UpdateWorkspaceDto, Workspace>();
            CreateMap<UpdateWorkspaceDto, WorkspaceDto>();
            CreateMap<UpdateAvatarWorkspaceDto, Workspace>();
            CreateMap<UpdateAvatarWorkspaceDto, WorkspaceDto>();
            
            CreateMap<Channel, ChannelDto>()
                .IncludeBase<FullAuditedEntity, FullAuditedDto>()
                .ForMember(x => x.ChannelMembers, opt => opt.MapFrom(src => src.ChannelMembers.Select(x => x.UserId)));
            CreateMap<CreateChannelDto, Channel>();
            CreateMap<CreateChannelDto, ChannelDto>();
            CreateMap<UpdateChannelDto, Channel>();
            CreateMap<UpdateChannelDto, ChannelDto>();
            
        }       
    }
}