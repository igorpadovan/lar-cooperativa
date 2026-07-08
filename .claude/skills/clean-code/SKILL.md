---
name: clean-code
description: Padrões de Clean Code do projeto. Usar sempre que for escrever ou revisar código C# neste repositório.
---

# Clean Code

Diretrizes obrigatórias ao escrever código neste projeto.

## Nomenclatura
- Nomes revelam intenção: `pessoasAtivas`, não `lista` ou `temp`. Nada de abreviações (`qtd`, `dt`, `pes`).
- Idioma: entidades e propriedades do domínio em português (seguem o modelo de dados: `Pessoa`, `Telefone`, `DataNascimento`); termos técnicos em inglês (`Repository`, `Service`, `GetByIdAsync`).
- Convenções C#: `PascalCase` para tipos, métodos e propriedades; `camelCase` para variáveis e parâmetros; `_camelCase` para campos privados; sufixo `Async` em métodos assíncronos.
- Booleanos leem como pergunta: `EstaAtivo`, `PossuiTelefone`, `isValid`.

## Funções e métodos
- Pequenos e com uma única responsabilidade — se o nome precisa de "E"/"And", divida.
- Poucos parâmetros (até 3); acima disso, crie um objeto (record) que os agrupe.
- Evite flags booleanas como parâmetro — prefira dois métodos com nomes claros.
- Early return em vez de aninhamento profundo (`if` invertido + `return`).
- Sem efeitos colaterais escondidos: um método `Get...` não altera estado.

## Comentários
- Código autoexplicativo em vez de comentário. Comentário só para o *porquê* (restrição, decisão, workaround), nunca para o *o quê*.
- Sem código comentado/morto — o Git guarda o histórico.

## Formatação e organização
- Um tipo público por arquivo; nome do arquivo = nome do tipo.
- `using` implícitos habilitados; remover `using` não utilizados.
- Nullable habilitado (`<Nullable>enable</Nullable>`): não suprimir avisos com `!` sem justificativa.

## Tratamento de erros
- Exceções para condições excepcionais; para fluxo esperado (ex.: registro não encontrado), retornar resultado explícito (`null`, `Result`, `404` no controller).
- Nunca capturar `Exception` genérica para silenciar (`catch {}` proibido).
- Validar entrada nas bordas (DTOs/requests com DataAnnotations ou FluentValidation); o domínio assume dados já validados ou se protege com invariantes.

## Referência rápida
Antes de finalizar qualquer alteração, pergunte:
1. Os nomes contam a história sem precisar ler a implementação?
2. Cada método faz uma coisa só?
3. Há duplicação que valha extrair (regra de 3)?
4. Um leitor novo entenderia sem comentários?
5. Os eventos de negócio estão logados conforme a skill [logging](../logging/SKILL.md)?
