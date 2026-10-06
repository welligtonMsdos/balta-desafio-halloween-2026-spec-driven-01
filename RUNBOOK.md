# Execução da API

## Pré-requisitos

- Docker Desktop em execução.
- Docker Compose v2.

## Iniciar o ambiente

```powershell
docker compose up --build
```

A API fica disponível em `http://localhost:8080` e a documentação interativa Scalar em `http://localhost:8080/scalar`. O PostgreSQL usa `tmpfs`, portanto nenhum dado é persistido após a remoção do contêiner.

## Executar testes

```powershell
dotnet test Auth.slnx --no-restore
docker compose -f docker-compose.test.yml up --build --abort-on-container-exit
```

Os testes de integração executados no Docker recebem a connection string do PostgreSQL efêmero e validam a API por HTTP.

## Configurações obrigatórias

As variáveis devem ser fornecidas pelo ambiente, nunca versionadas com segredos:

- `ConnectionStrings__Postgres`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Key` (mínimo de 32 bytes)
- `Jwt__ExpiresInMinutes`

## Encerrar o ambiente

```powershell
docker compose down
docker compose -f docker-compose.test.yml down
```
