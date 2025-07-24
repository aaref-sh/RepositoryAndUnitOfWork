using Core.Filters;
using Core.Paginated;
using Microsoft.AspNetCore.Mvc;
using MainService.Presistance.Entities;
using MainService.Application.DTOs.Role;
using MainService.Application.Services.Interfaces;
using AutoMapper;

namespace MainService.API.Controllers;

[Route("api/Role")]
public class RoleController(IRoleService service, IMapper mapper) : ControllerBase
{
    [HttpGet("AllLite")]
    public async Task<ActionResult<PaginatedResult<RoleLiteDto>>> AllLite(BaseFilter<Role> filter)
    {
        filter.SetFilters();
        filter.SetOrder();

        var items = await service.GetList(filter);
        PaginatedResult<RoleLiteDto> paginated = new()
        {
            Data = [.. items.Select(mapper.Map<RoleLiteDto>)],
            CurrentPage = items.Page,
            ItemsPerPage = items.PerPage,
            TotalItems = items.TotalCount
        };
        return Ok(paginated);
    }

}
