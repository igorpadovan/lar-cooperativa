# Lar Cooperativa

WebAPI REST em .NET 10 com PostgreSQL 18, desenvolvida como avaliação técnica. Implementa CRUD para as entidades **Pessoa** e **Telefone**.

## Requisitos

Apenas **Docker** com o plugin **Docker Compose** (incluído no Docker Desktop para Windows/macOS e no pacote `docker-compose-plugin` no Linux). Não é necessário ter o SDK .NET nem o PostgreSQL instalados na máquina — tudo roda em containers, em qualquer sistema operacional.

## Configuração

Usuário, senha e portas ficam no arquivo `.env` (não versionado). Crie o seu a partir do exemplo:

```bash
cp .env.example .env        # Linux/macOS
```

```powershell
Copy-Item .env.example .env # Windows (PowerShell)
```

| Variável             | Padrão            | Descrição                        |
| -------------------- | ----------------- | -------------------------------- |
| `POSTGRES_USER`      | `lar`             | Usuário do PostgreSQL            |
| `POSTGRES_PASSWORD`  | `lar`             | Senha do PostgreSQL              |
| `POSTGRES_DB`        | `lar_cooperativa` | Nome do banco de desenvolvimento |
| `POSTGRES_HOST_PORT` | `5433`            | Porta do PostgreSQL no host      |
| `API_HOST_PORT`      | `8081`            | Porta da API no host             |

> O `compose.yaml` tem esses mesmos valores como fallback, então o ambiente também sobe sem o `.env` — o arquivo só é necessário para customizar.

## Ambiente de desenvolvimento

Subir API + PostgreSQL (na primeira execução a imagem será construída e os pacotes restaurados — pode levar alguns minutos):

```bash
docker compose up --build
```

Para rodar em segundo plano, adicione `-d` e acompanhe os logs com:

```bash
docker compose logs -f api
```

Derrubar o ambiente:

```bash
docker compose down            # mantém os dados do banco
docker compose down --volumes  # apaga também os volumes (banco e cache de pacotes)
```

| Serviço          | Endereço (com as portas padrão)                                       |
| ---------------- | --------------------------------------------------------------------- |
| API              | http://localhost:8081                                                  |
| Swagger UI       | http://localhost:8081/swagger — para testar os endpoints manualmente   |
| OpenAPI (spec)   | http://localhost:8081/openapi/v1.json                                  |
| PostgreSQL (dev) | `localhost:5433` — banco, usuário e senha conforme o `.env`            |

A API roda com `dotnet watch` e o código-fonte é montado por volume: qualquer alteração é recompilada automaticamente (hot reload).

## Testes

O script de inicialização do PostgreSQL cria um banco separado para testes de integração, com o sufixo `_test` (padrão: `lar_cooperativa_test`). A suíte roda em container:

```bash
docker compose --profile test run --rm tests
```

## Outros comandos úteis

Abrir um `psql` no banco de desenvolvimento:

```bash
docker compose exec postgres psql -U lar -d lar_cooperativa
```

Executar comandos do `dotnet` CLI sem ter o SDK instalado (exemplo: adicionar um pacote):

```bash
docker run --rm -it -v "$PWD:/app" -w /app -v lar-cooperativa_nuget-cache:/root/.nuget/packages mcr.microsoft.com/dotnet/sdk:10.0 dotnet add src/LarCooperativa.Api package Npgsql
```

> No Windows (PowerShell), substitua `"$PWD:/app"` por `"${PWD}:/app"`.

## API

Endpoints de Pessoa (exemplos prontos em [LarCooperativa.Api.http](src/LarCooperativa.Api/LarCooperativa.Api.http)):

| Método   | Rota                | Respostas                          |
| -------- | ------------------- | ---------------------------------- |
| `GET`    | `/api/pessoas`      | `200`                              |
| `GET`    | `/api/pessoas/{id}` | `200` · `404`                      |
| `POST`   | `/api/pessoas`      | `201` · `400` · `409` (CPF em uso) |
| `PUT`    | `/api/pessoas/{id}` | `200` · `400` · `404` · `409`      |
| `DELETE` | `/api/pessoas/{id}` | `204` · `404`                      |

Telefones de uma pessoa (relação 1:N, removidos em cascata com a pessoa):

| Método   | Rota                                     | Respostas                             |
| -------- | ---------------------------------------- | ------------------------------------- |
| `GET`    | `/api/pessoas/{pessoaId}/telefones`      | `200` · `404` (pessoa)                |
| `GET`    | `/api/pessoas/{pessoaId}/telefones/{id}` | `200` · `404`                         |
| `POST`   | `/api/pessoas/{pessoaId}/telefones`      | `201` · `400` · `404` · `409` (número em uso) |
| `PUT`    | `/api/pessoas/{pessoaId}/telefones/{id}` | `200` · `400` · `404` · `409`         |
| `DELETE` | `/api/pessoas/{pessoaId}/telefones/{id}` | `204` · `404`                         |

Regras de negócio:

- **Pessoa**: CPF validado (dígitos verificadores) e único, aceito com ou sem máscara e armazenado normalizado; data de nascimento não pode estar no futuro; pessoa é criada ativa.
- **Telefone**: tipo `Celular`, `Residencial` ou `Comercial`; número aceito com máscara e armazenado só com dígitos (DDD + número: 11 dígitos para celular, 10 para os demais); uma pessoa não pode ter o mesmo número repetido.

Erros seguem o formato Problem Details (RFC 9457).

As migrations do EF Core são aplicadas automaticamente na inicialização da aplicação.

## Estrutura

```
├── .env.example                    # modelo de configuração (usuário, senha e portas)
├── compose.yaml                    # PostgreSQL 18 + API (dev) + runner de testes
├── Dockerfile                      # multi-stage: dev (watch) / build / runtime
├── docker/postgres/init/           # scripts de inicialização do banco
├── src/LarCooperativa.Api/
│   ├── Controllers/                # camada HTTP (rotas, status codes)
│   ├── Services/                   # regras de negócio
│   ├── Domain/                     # entidades, value objects e contratos de repositório
│   ├── Data/                       # EF Core: DbContext, configurações, repositórios, migrations
│   ├── Contracts/                  # DTOs de request/response
│   └── Common/                     # Result/Error compartilhados
└── tests/
    ├── LarCooperativa.UnitTests/         # regras de negócio (xUnit + NSubstitute)
    └── LarCooperativa.IntegrationTests/  # endpoints ponta a ponta (WebApplicationFactory)
```
