using AutoMapper;
using PBL6.Application.Contract.Examples.Dtos;
using PBL6.Application.Contract;
using PBL6.Domain.Models;
using PBL6.Domain.Models.Common;

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
        }
    }
}