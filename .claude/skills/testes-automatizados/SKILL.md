---
name: testes-automatizados
description: Padrões de testes automatizados do projeto. Usar sempre que implementar funcionalidade nova, corrigir bug ou alterar comportamento existente.
---

# Testes automatizados

Toda funcionalidade nova nasce com testes. Bug corrigido ganha antes um teste que o reproduz.

## TDD — obrigatório em qualquer implementação
Seguir o ciclo **Red → Green → Refactor**:
1. **Red**: escreva primeiro o teste que descreve o comportamento desejado e rode a suíte — ele deve **falhar** (se passar de cara, o teste não testa nada novo).
2. **Green**: escreva o código **mínimo** para o teste passar. Sem antecipar nada além do que o teste exige (ver [yagni](../yagni/SKILL.md)).
3. **Refactor**: com a suíte verde, melhore nomes, remova duplicação e ajuste o design ([clean-code](../clean-code/SKILL.md), [solid](../solid/SKILL.md)). Rode a suíte de novo ao final.

Regras práticas:
- Nunca escrever código de produção sem um teste falhando que o justifique.
- Para bug: primeiro o teste que reproduz o defeito (Red), depois a correção (Green).
- Commits idealmente com a suíte verde; o par teste + implementação anda junto no mesmo commit.
- Regras de negócio nascem via TDD de unidade; o contrato HTTP nasce via TDD de integração (o teste do endpoint vem antes do controller).

## Stack
- **xUnit** como framework de testes.
- **Testes de unidade**: regras de negócio (entidades, services) — sem banco, sem HTTP. Dependências dubladas com **NSubstitute**.
- **Testes de integração**: endpoints da API de ponta a ponta com `WebApplicationFactory` (`Microsoft.AspNetCore.Mvc.Testing`), contra o banco `${POSTGRES_DB}_test` criado pelo init do PostgreSQL (connection string vem da env `ConnectionStrings__Default` — ver `compose.yaml`, serviço `tests`).

## Organização
```
tests/
├── LarCooperativa.UnitTests/         # espelha a estrutura de src/
└── LarCooperativa.IntegrationTests/  # um arquivo por recurso/endpoint
```
- Projetos de teste entram na solution (`dotnet sln add`).
- Classe de teste nomeada `<TipoTestado>Tests`; arquivo espelha o caminho do tipo em `src/`.

## Padrões de escrita
- Nome do teste descreve comportamento: `MetodoOuAcao_Cenario_ResultadoEsperado` (ex.: `Criar_ComCpfDuplicado_RetornaConflict`).
- Estrutura **AAA** (Arrange / Act / Assert), com uma linha em branco separando os blocos.
- Um comportamento por teste; várias asserções só se verificam o mesmo resultado.
- Casos parametrizados com `[Theory]`/`[InlineData]` em vez de copiar testes.
- Teste independente e determinístico: sem ordem implícita, sem `DateTime.Now` direto (injete/controle o relógio quando relevante), sem dados compartilhados entre testes — integração limpa/isola os dados que cria.
- Teste também os caminhos de erro (404, validação inválida, CPF duplicado), não só o caminho feliz.

## O que testar (prioridade)
1. Regras de negócio e invariantes do domínio (unidade).
2. Contrato dos endpoints: status codes, corpo, validação (integração).
3. Não testar framework (EF Core, ASP.NET) nem getters/setters triviais.

## Execução
Suíte completa em container (não requer SDK no host):
```bash
docker compose --profile test run --rm tests
```
Antes de dar qualquer tarefa como concluída: rodar a suíte e ela deve estar **verde**. Teste quebrado não se comenta nem se marca com `Skip` — conserta-se o código ou o teste.
