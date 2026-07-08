using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LarCooperativa.Api.Controllers;

[ApiController]
[Route("api/pessoas")]
public class PessoasController(IPessoaService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<PessoaResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<PessoaResponse>>> GetAll(
        [FromQuery] PaginationQuery paginacao, CancellationToken cancellationToken)
    {
        var pagina = await service.GetAllAsync(paginacao, cancellationToken);
        return Ok(PagedResponse<PessoaResponse>.FromPage(pagina, PessoaResponse.FromDomain));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<PessoaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PessoaResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var pessoa = await service.GetByIdAsync(id, cancellationToken);
        return pessoa is null ? NotFound() : Ok(PessoaResponse.FromDomain(pessoa));
    }

    [HttpPost]
    [ProducesResponseType<PessoaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PessoaResponse>> Create(
        CreatePessoaRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return this.ToProblem(result.Error!);
        }

        var response = PessoaResponse.FromDomain(result.Value);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<PessoaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PessoaResponse>> Update(
        Guid id, UpdatePessoaRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess
            ? Ok(PessoaResponse.FromDomain(result.Value))
            : this.ToProblem(result.Error!);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToProblem(result.Error!);
    }
}
