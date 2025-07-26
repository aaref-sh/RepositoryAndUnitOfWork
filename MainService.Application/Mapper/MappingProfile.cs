using AutoMapper;
using AutoMapper.Internal;
using Core.DTO;
using Core.Entities;
using System.Collections;
using System.Reflection;

namespace MainService.Application.Mapper;

internal class MappingProfile : Profile
{
    private static readonly Type[] types = [.. Assembly.Load("MainService.Presistance").GetTypes().Where(t => t.IsAssignableTo(typeof(IBaseEntity)))];
    public MappingProfile()
    {
        var allTypes = Assembly.GetExecutingAssembly().GetTypes();

        var DetailsDtoTypes = allTypes.Where(t => t.IsSubclassOf(typeof(BaseDetailsDto))).ToArray();
        var CreateDtoTypes = allTypes.Where(t => t.IsSubclassOf(typeof(BaseCreateDto))).ToArray();
        var ListDtoTypes = allTypes.Where(t => t.IsSubclassOf(typeof(BaseListDto))).ToArray();
        var LiteDtoTypes = allTypes.Where(t => t.IsSubclassOf(typeof(BaseLiteDto))).ToArray();
        var UpdateDtoTypes = allTypes.Where(t => t.IsSubclassOf(typeof(BaseUpdateDto))).ToArray();

        foreach (var entityType in types)
        {
            var CreateDtoType = CreateDtoTypes.FirstOrDefault(t => t.Name == $"{entityType.Name}CreateDto");
            var UpdateDtoType = UpdateDtoTypes.FirstOrDefault(t => t.Name == $"{entityType.Name}UpdateDto");
            var DetailsDtoType = DetailsDtoTypes.FirstOrDefault(t => t.Name == $"{entityType.Name}DetailsDto");
            var ListDtoType = ListDtoTypes.FirstOrDefault(t => t.Name == $"{entityType.Name}ListDto");
            var LiteDtoType = LiteDtoTypes.FirstOrDefault(t => t.Name == $"{entityType.Name}LiteDto");

            if (CreateDtoType != null)
                CreateMap(CreateDtoType, entityType);

            if (UpdateDtoType != null)
                CreateMap(UpdateDtoType, entityType);

            if (DetailsDtoType != null)
                CreateMap(entityType, DetailsDtoType).IncludeAllDerived();

            if (ListDtoType != null)
                CreateMap(entityType, ListDtoType).IncludeAllDerived();

            if (LiteDtoType != null)
                CreateMap(entityType, LiteDtoType).IncludeAllDerived();

        }
    }
}