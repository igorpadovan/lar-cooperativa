using LarCooperativa.Api.Common;
using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Domain;

namespace LarCooperativa.Api.Services;

public sealed class PessoaService(IPessoaRepository repository, ILogger<PessoaService> logger) : IPessoaService
{
    public Task<Page<Pessoa>> GetAllAsync(PaginationQuery paginacao, CancellationToken cancellationToken) =>
        repository.GetPageAsync(paginacao.Pagina, paginacao.TamanhoPagina, cancellationToken);

    public Task<Pessoa?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(id, cancellationToken);

    public async Task<Result<Pessoa>> CreateAsync(CreatePessoaRequest request, CancellationToken cancellationToken)
    {
        var cpf = Cpf.TryCreate(request.Cpf);
        if (cpf is null)
        {
            return Result<Pessoa>.Failure(PessoaErrors.CpfInvalido);
        }

        if (await repository.ExistsByCpfAsync(cpf, excludeId: null, cancellationToken))
        {
            logger.LogWarning("Tentativa de criar pessoa com CPF já cadastrado");
            return Result<Pessoa>.Failure(PessoaErrors.CpfDuplicado);
        }

        var pessoa = new Pessoa(request.Nome, cpf, request.DataNascimento);
        await repository.AddAsync(pessoa, cancellationToken);
        logger.LogInformation("Pessoa {PessoaId} criada", pessoa.Id);

        return Result<Pessoa>.Success(pessoa);
    }

    public async Task<Result<Pessoa>> UpdateAsync(Guid id, UpdatePessoaRequest request, CancellationToken cancellationToken)
    {
        var pessoa = await repository.GetByIdAsync(id, cancellationToken);
        if (pessoa is null)
        {
            return Result<Pessoa>.Failure(PessoaErrors.NaoEncontrada);
        }

        var cpf = Cpf.TryCreate(request.Cpf);
        if (cpf is null)
        {
            return Result<Pessoa>.Failure(PessoaErrors.CpfInvalido);
        }

        if (await repository.ExistsByCpfAsync(cpf, excludeId: id, cancellationToken))
        {
            logger.LogWarning("Tentativa de atualizar pessoa {PessoaId} com CPF de outra pessoa", id);
            return Result<Pessoa>.Failure(PessoaErrors.CpfDuplicado);
        }

        pessoa.Atualizar(request.Nome, cpf, request.DataNascimento, request.EstaAtivo);
        await repository.UpdateAsync(pessoa, cancellationToken);
        logger.LogInformation("Pessoa {PessoaId} atualizada", pessoa.Id);

        return Result<Pessoa>.Success(pessoa);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var pessoa = await repository.GetByIdAsync(id, cancellationToken);
        if (pessoa is null)
        {
            return Result.Failure(PessoaErrors.NaoEncontrada);
        }

        await repository.DeleteAsync(pessoa, cancellationToken);
        logger.LogInformation("Pessoa {PessoaId} removida", pessoa.Id);

        return Result.Success();
    }
}
