---
name: yagni
description: YAGNI — You Aren't Gonna Need It. Usar ao decidir escopo de qualquer implementação ou refatoração neste projeto.
---

# YAGNI

Implemente apenas o que o requisito atual pede. Generalização especulativa é dívida, não investimento.

## Regras
- **Não criar** camadas, interfaces, abstrações ou parâmetros "para o futuro". Crie quando o segundo caso de uso concreto aparecer.
- **Não adicionar** pacotes NuGet para resolver o que a BCL/ASP.NET já resolve bem (ex.: não adicionar AutoMapper para mapear 4 propriedades; um método de extensão basta).
- **Não configurar** o que não é usado (feature flags, opções, ambientes hipotéticos).
- **Não expor** endpoints, propriedades ou métodos que nenhum consumidor atual chama.
- Duplicação pequena é aceitável até a **regra de 3**: na terceira ocorrência, extraia.

## Como decidir
Antes de adicionar qualquer coisa, responda:
1. Algum requisito **atual** precisa disso? (Se a resposta começa com "se um dia…", não crie.)
2. Qual o custo de adicionar depois? Se for baixo (na maioria dos casos é), adie.
3. Isso simplifica ou apenas "prepara"? Preparação sem uso é complexidade gratuita.

## O que YAGNI **não** é
- Não é desculpa para ignorar [SOLID](../solid/SKILL.md) ou [clean code](../clean-code/SKILL.md): a separação Controller → Service → Repository e os [testes automatizados](../testes-automatizados/SKILL.md) fazem parte do requisito atual (avaliação técnica), portanto não são especulação.
- Não é desculpa para gambiarras: simples ≠ malfeito. Escolha a solução mais simples **bem feita**.

## Aplicação neste projeto
- Começamos apenas com a entidade `Pessoa`; nada de criar estruturas genéricas antecipando `Telefone` — quando ela chegar, refatoramos o que de fato repetir.
- Paginação, cache, mensageria, CQRS, MediatR: só se um requisito concreto pedir.
