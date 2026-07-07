---
name: solid
description: Princípios SOLID aplicados à arquitetura deste projeto .NET. Usar ao criar/alterar classes, serviços, repositórios e controllers.
---

# SOLID

Como aplicar cada princípio neste projeto (WebAPI .NET + PostgreSQL).

## S — Single Responsibility
- Cada classe tem um único motivo para mudar:
  - **Controller**: só HTTP (rotas, status codes, binding). Sem regra de negócio, sem acesso a dados.
  - **Service/UseCase**: orquestra a regra de negócio.
  - **Repository**: só persistência.
  - **Entidade**: estado + invariantes do domínio.
- Sinal de violação: classe com "And" implícito no que faz, ou métodos que não usam os mesmos campos.

## O — Open/Closed
- Estender comportamento por composição/novas implementações, não editando cadeias de `if/else` ou `switch` sobre tipos.
- Ex.: validações novas entram como novo validator registrado no DI, não como mais um `if` no service.

## L — Liskov Substitution
- Implementações de uma interface devem honrar o contrato: mesmas pré/pós-condições, sem lançar `NotImplementedException`, sem exigir downcast.
- Se uma implementação precisa "não suportar" parte da interface, a interface está grande demais (vá para o I).

## I — Interface Segregation
- Interfaces pequenas e focadas no consumidor: `IPessoaRepository` com o que o caso de uso precisa, não um `IRepository<T>` genérico com 20 métodos dos quais usa 3.
- Aceitável começar sem interface e extraí-la quando surgir o segundo consumidor ou a necessidade de mock.

## D — Dependency Inversion
- Camadas de alto nível dependem de abstrações; implementações concretas são detalhes registrados no DI (`Program.cs`).
- Dependências sempre via construtor (constructor injection); nada de `new` em serviço/repositório dentro de outra classe, nem service locator.
- Lifetimes: `Scoped` para DbContext/repositórios/services por padrão; `Singleton` só para estado imutável/thread-safe.

## Pragmatismo
SOLID serve à manutenibilidade, não ao cerimonial. Em caso de conflito com [YAGNI](../yagni/SKILL.md), prefira a solução mais simples que preserve a separação Controller → Service → Repository.
