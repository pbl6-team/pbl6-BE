using AutoMapper;
using PBL6.Application.Contract.Examples.Dtos;
using PBL6.Application.Contract;
using PBL6.Domain.Models;
using PBL6.Domain.Models.Common;
using PBL6.Domain.Models.Users;
using PBL6.Application.Contract.Workspaces.Dtos;

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
                .IncludeBase<FullAuditedEntity, FullAuditedDto>();
            CreateMap<CreateWorkspaceDto, Workspace>();
            CreateMap<CreateWorkspaceDto, WorkspaceDto>();
            CreateMap<UpdateWorkspaceDto, Workspace>();
            CreateMap<UpdateWorkspaceDto, WorkspaceDto>();
            CreateMap<UpdateAvatarWorkspaceDto, Workspace>();
            CreateMap<UpdateAvatarWorkspaceDto, WorkspaceDto>();
            
        }
    }
}