using LarCooperativa.Api.Common;
using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Domain;

namespace LarCooperativa.Api.Services;

public sealed class PessoaService(IPessoaRepository repository) : IPessoaService
{
    public Task<IReadOnlyList<Pessoa>> GetAllAsync(CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);

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
            return Result<Pessoa>.Failure(PessoaErrors.CpfDuplicado);
        }

        var pessoa = new Pessoa(request.Nome, cpf, request.DataNascimento);
        await repository.AddAsync(pessoa, cancellationToken);

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
            return Result<Pessoa>.Failure(PessoaErrors.CpfDuplicado);
        }

        pessoa.Atualizar(request.Nome, cpf, request.DataNascimento, request.EstaAtivo);
        await repository.UpdateAsync(pessoa, cancellationToken);

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

        return Result.Success();
    }
}
