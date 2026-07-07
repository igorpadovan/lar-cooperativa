using LarCooperativa.Api.Common;
using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LarCooperativa.Api.Controllers;

[ApiController]
[Route("api/pessoas")]
public class PessoasController(IPessoaService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<PessoaResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PessoaResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var pessoas = await service.GetAllAsync(cancellationToken);
        return Ok(pessoas.Select(PessoaResponse.FromDomain));
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
            return FromError(result.Error!);
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
            : FromError(result.Error!);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : FromError(result.Error!);
    }

    private ObjectResult FromError(Error error) => Problem(
        title: error.Code,
        detail: error.Message,
        statusCode: error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        });
}
