using AutoMapper;
using Core.BaseService;
using Core.DTO;
using Core.Entities;
using Core.Filters;
using Core.Paginated;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.BaseController;

[Authorize]
public class BaseGetController<T, TDetailsDto, TLiteDto, TListDto>
    (IBaseService<T> service, IMapper mapper, IHttpContextAccessor httpContextAccessor) : ControllerBase
    where T : class, IBaseEntity
{
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    [HttpGet("{Id}")]
    public virtual async Task<ActionResult<TDetailsDto>> Get(long Id)
    {
        var entity = await service.GetById(Id);
        var res = mapper.Map<TDetailsDto>(entity);
        return Ok(res);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("All")]
    public virtual async Task<ActionResult<List<TListDto>>> All(BaseFilter<T> filter)
    {
        InitFilter(filter);

        var entitis = await service.GetList(filter);
        var items = mapper.Map<List<TListDto>>(entitis);
        PaginatedResult<TListDto> res = new(items, entitis.Page, entitis.PerPage, entitis.TotalCount);
        return Ok(res);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("AllLite")]
    public virtual async Task<ActionResult<List<TListDto>>> AllLite(BaseFilter<T> filter)
    {
        InitFilter(filter);

        var entitis = await service.GetList(filter);
        var items = mapper.Map<List<TLiteDto>>(entitis);
        PaginatedResult<TLiteDto> res = new(items, entitis.Page, entitis.PerPage, entitis.TotalCount);
        return Ok(res);
    }

    protected static void InitFilter(BaseFilter<T> filter)
    {
        filter.SetFilters();
        filter.SetOrder();
    }

}


[Authorize]
public class BaseController<T, TDetailsDto, TCreateDto, TUpdateDto, TLiteDto, TListDto>(IBaseService<T> service, IMapper mapper, IHttpContextAccessor httpContextAccessor) 
    : BaseGetController<T, TDetailsDto, TLiteDto, TListDto>(service, mapper, httpContextAccessor)
    where T : class, IBaseEntity
    where TCreateDto : BaseCreateDto, new()
    where TUpdateDto : BaseUpdateDto, new()
{

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    [HttpDelete("Delete/{Id}")]
    public virtual async Task<IActionResult> Delete(long id)
    {
        await service.Delete(id);
        return Ok();
    }


    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut("Update")]
    public virtual async Task<IActionResult> Update([FromBody] TUpdateDto updateDto)
    {
        await service.Update(updateDto);
        return Ok();
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("Create")]
    public virtual async Task<IActionResult> Create([FromBody] TCreateDto createDto)
    {
        await service.Create(createDto);
        return Created();
    }

}
