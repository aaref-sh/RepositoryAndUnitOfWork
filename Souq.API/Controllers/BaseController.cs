using AutoMapper;
using Core.BaseService;
using Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Souq.API.Controllers;


public class BaseController<T, TDetailsDto, TCreateDto, TUpdateDto, TLiteDto, TListDto>
    (IBaseService<T> service, IMapper mapper) : ControllerBase 
    where T : class, IBaseEntity
{

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)] [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    [HttpGet("{Id}")]
    public async Task<ActionResult<TDetailsDto>> Get(long Id)
    {
        var entity = await service.GetById(Id);
        var res = mapper.Map<TDetailsDto>(entity);
        return Ok(res);
    }
}
