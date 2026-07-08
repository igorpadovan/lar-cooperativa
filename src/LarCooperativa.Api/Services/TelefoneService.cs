using LarCooperativa.Api.Common;
using LarCooperativa.Api.Contracts;
using LarCooperativa.Api.Domain;

namespace LarCooperativa.Api.Services;

public sealed class TelefoneService(
    ITelefoneRepository telefoneRepository,
    IPessoaRepository pessoaRepository) : ITelefoneService
{
    public async Task<Result<IReadOnlyList<Telefone>>> GetAllAsync(Guid pessoaId, CancellationToken cancellationToken)
    {
        if (!await PessoaExisteAsync(pessoaId, cancellationToken))
        {
            return Result<IReadOnlyList<Telefone>>.Failure(PessoaErrors.NaoEncontrada);
        }

        var telefones = await telefoneRepository.GetAllByPessoaAsync(pessoaId, cancellationToken);
        return Result<IReadOnlyList<Telefone>>.Success(telefones);
    }

    public async Task<Result<Telefone>> GetByIdAsync(Guid pessoaId, Guid id, CancellationToken cancellationToken)
    {
        var telefone = await telefoneRepository.GetByIdAsync(pessoaId, id, cancellationToken);
        return telefone is null
            ? Result<Telefone>.Failure(TelefoneErrors.NaoEncontrado)
            : Result<Telefone>.Success(telefone);
    }

    public async Task<Result<Telefone>> CreateAsync(
        Guid pessoaId, CreateTelefoneRequest request, CancellationToken cancellationToken)
    {
        if (!await PessoaExisteAsync(pessoaId, cancellationToken))
        {
            return Result<Telefone>.Failure(PessoaErrors.NaoEncontrada);
        }

        var numero = NumeroTelefone.TryNormalizar(request.Numero, request.Tipo);
        if (numero is null)
        {
            return Result<Telefone>.Failure(TelefoneErrors.NumeroInvalido);
        }

        if (await telefoneRepository.ExistsByNumeroAsync(pessoaId, numero, excludeId: null, cancellationToken))
        {
            return Result<Telefone>.Failure(TelefoneErrors.NumeroDuplicado);
        }

        var telefone = new Telefone(pessoaId, request.Tipo, numero);
        await telefoneRepository.AddAsync(telefone, cancellationToken);

        return Result<Telefone>.Success(telefone);
    }

    public async Task<Result<Telefone>> UpdateAsync(
        Guid pessoaId, Guid id, UpdateTelefoneRequest request, CancellationToken cancellationToken)
    {
        var telefone = await telefoneRepository.GetByIdAsync(pessoaId, id, cancellationToken);
        if (telefone is null)
        {
            return Result<Telefone>.Failure(TelefoneErrors.NaoEncontrado);
        }

        var numero = NumeroTelefone.TryNormalizar(request.Numero, request.Tipo);
        if (numero is null)
        {
            return Result<Telefone>.Failure(TelefoneErrors.NumeroInvalido);
        }

        if (await telefoneRepository.ExistsByNumeroAsync(pessoaId, numero, excludeId: id, cancellationToken))
        {
            return Result<Telefone>.Failure(TelefoneErrors.NumeroDuplicado);
        }

        telefone.Atualizar(request.Tipo, numero);
        await telefoneRepository.UpdateAsync(telefone, cancellationToken);

        return Result<Telefone>.Success(telefone);
    }

    public async Task<Result> DeleteAsync(Guid pessoaId, Guid id, CancellationToken cancellationToken)
    {
        var telefone = await telefoneRepository.GetByIdAsync(pessoaId, id, cancellationToken);
        if (telefone is null)
        {
            return Result.Failure(TelefoneErrors.NaoEncontrado);
        }

        await telefoneRepository.DeleteAsync(telefone, cancellationToken);

        return Result.Success();
    }

    private async Task<bool> PessoaExisteAsync(Guid pessoaId, CancellationToken cancellationToken) =>
        await pessoaRepository.GetByIdAsync(pessoaId, cancellationToken) is not null;
}
