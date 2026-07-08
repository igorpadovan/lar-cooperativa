---
name: logging
description: Padrões de logging do projeto — requisito obrigatório em toda implementação. Usar ao criar ou alterar services, controllers ou qualquer fluxo de negócio.
---

# Logging

**Requisito obrigatório**: toda funcionalidade implementada registra logs dos seus eventos relevantes. Não existe feature "pronta" sem logs — inclua-os na fase Green do TDD, junto com a implementação.

## Como
- `ILogger<T>` injetado por construtor (logging nativo do .NET; não adicionar pacotes de logging sem necessidade concreta — ver [yagni](../yagni/SKILL.md)).
- Logging **estruturado** com message templates: `logger.LogInformation("Pessoa {PessoaId} criada", pessoa.Id)`. Nunca interpolação (`$"..."`) — quebra a agregação por template e o log estruturado.
- Onde logar:
  - **Services**: eventos de negócio (é a casa principal dos logs).
  - **Composition root** (`Program.cs`): eventos de inicialização (seed, migrations).
  - **Controllers e repositórios não logam**: o pipeline HTTP e o EF Core já cobrem requisição e SQL.

## Níveis
- `Information`: evento de negócio bem-sucedido que **muda estado** (criou/atualizou/removeu/autenticou) — sempre com os ids envolvidos.
- `Warning`: falha esperada relevante para operação/segurança (conflito de dados, tentativa de login inválida).
- `Error`: falha inesperada — em geral coberta pelo exception handler global; use apenas ao capturar exceção deliberadamente.
- `Debug`: detalhe de diagnóstico; não poluir `Information`.
- Leituras (GET) não geram log de negócio.

## Dados sensíveis — nunca logar
- Senhas, hashes, tokens/JWT, connection strings.
- Dados pessoais (LGPD): CPF, data de nascimento, número de telefone. Logue **ids**, não os dados: `{PessoaId}`, nunca `{Cpf}`.
- Nome de usuário em tentativa de login é aceitável (diagnóstico de segurança).

## Testes
- Logs não são alvo de testes automatizados (não asserte chamadas de logger) — ver [testes-automatizados](../testes-automatizados/SKILL.md).
- Nos testes de unidade, injete `NullLogger<T>.Instance` (`Microsoft.Extensions.Logging.Abstractions`).
