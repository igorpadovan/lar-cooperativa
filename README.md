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

## Estrutura

```
├── .env.example                # modelo de configuração (usuário, senha e portas)
├── compose.yaml                # PostgreSQL 18 + API (dev) + runner de testes
├── Dockerfile                  # multi-stage: dev (watch) / build / runtime
├── docker/postgres/init/       # scripts de inicialização do banco
└── src/LarCooperativa.Api/     # WebAPI
```
