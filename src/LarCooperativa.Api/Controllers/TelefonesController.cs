using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LarCooperativa.Api.Controllers;

[ApiController]
[Route("api/pessoas/{pessoaId:guid}/telefones")]
public class TelefonesController(ITelefoneService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<TelefoneResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TelefoneResponse>>> GetAll(
        Guid pessoaId, CancellationToken cancellationToken)
    {
        var result = await service.GetAllAsync(pessoaId, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value.Select(TelefoneResponse.FromDomain))
            : this.ToProblem(result.Error!);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TelefoneResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TelefoneResponse>> GetById(
        Guid pessoaId, Guid id, CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(pessoaId, id, cancellationToken);
        return result.IsSuccess
            ? Ok(TelefoneResponse.FromDomain(result.Value))
            : this.ToProblem(result.Error!);
    }

    [HttpPost]
    [ProducesResponseType<TelefoneResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TelefoneResponse>> Create(
        Guid pessoaId, CreateTelefoneRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(pessoaId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return this.ToProblem(result.Error!);
        }

        var response = TelefoneResponse.FromDomain(result.Value);
        return CreatedAtAction(nameof(GetById), new { pessoaId, id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<TelefoneResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TelefoneResponse>> Update(
        Guid pessoaId, Guid id, UpdateTelefoneRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(pessoaId, id, request, cancellationToken);
        return result.IsSuccess
            ? Ok(TelefoneResponse.FromDomain(result.Value))
            : this.ToProblem(result.Error!);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid pessoaId, Guid id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(pessoaId, id, cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToProblem(result.Error!);
    }
}
