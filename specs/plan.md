# Plano técnico — Autenticação de Usuários

## Contexto

Será construída uma API HTTP mínima em C# sobre .NET 10 para cadastro, autenticação e CRUD de usuários, conforme `specs/spec.md`. A API exporá cadastro e login por e-mail e senha, listagem paginada de dados públicos e operações individuais de consulta, atualização e exclusão da própria conta. O login bem-sucedido devolverá um JSON Web Token (JWT) assinado, que representa a sessão autenticada.

Os dados serão armazenados em PostgreSQL executado no Docker Compose. Para cumprir o requisito de banco em memória, o diretório de dados do PostgreSQL será montado como `tmpfs` no contêiner. Assim, o banco é real, compatível com PostgreSQL e isolado no Docker, mas todo dado é perdido ao recriar ou encerrar o contêiner. Não haverá volume persistente.

O ambiente será autocontido: API, banco, execução de testes e suas configurações estarão no repositório e serão iniciados por Docker Compose. A aplicação não fará chamadas para APIs, serviços de identidade, bancos ou outros recursos externos.

## Arquitetura

A solução será organizada em projetos com responsabilidades explícitas:

- `src/Auth.Api`: projeto ASP.NET Core Minimal API. Contém endpoints, configuração de autenticação JWT, tratamento de erros e composição das dependências.
- `src/Auth.Application`: casos de uso de cadastro, login, consulta, listagem, atualização e exclusão de usuários; contratos de entrada e saída; validações de negócio e interfaces de persistência, senha e token.
- `src/Auth.Domain`: entidade `User`, regras invariantes do domínio e erros de negócio, sem dependência de ASP.NET Core, banco ou infraestrutura.
- `src/Auth.Infrastructure`: implementação EF Core/Npgsql do repositório, mapeamentos, migrações, hash/verificação de senha e geração de JWT.
- `tests/Auth.UnitTests`: testes unitários das regras de senha, normalização de e-mail e casos de uso, usando substitutos das interfaces.
- `tests/Auth.IntegrationTests`: testes ponta a ponta dos endpoints HTTP com PostgreSQL efêmero iniciado pelo Docker Compose de testes.

Fluxo de cadastro: endpoint → caso de uso → normalização e validação → verificação de unicidade → hash da senha → persistência transacional. Fluxo de login: endpoint → caso de uso → busca por e-mail normalizado → verificação do hash → emissão do JWT. Fluxos de CRUD: endpoint autenticado → validação do JWT e da titularidade quando individual → caso de uso → persistência ou retorno de dados públicos.

O banco será a fonte de verdade para a unicidade. Além da verificação feita pela aplicação, uma restrição única no campo normalizado do e-mail impedirá duplicidade mesmo em cadastros concorrentes.

## Decisões

- A API usará .NET 10, C# e ASP.NET Core Minimal API, mantendo a superfície HTTP pequena e apropriada ao escopo.
- O acesso a dados usará Entity Framework Core com o provedor Npgsql para PostgreSQL. Migrações versionadas criarão o schema ao iniciar o ambiente.
- O e-mail será removido de espaços nas extremidades e normalizado para minúsculas invariantes antes de validar, consultar e persistir. A unicidade será aplicada sobre esse valor normalizado.
- A senha será validada no servidor: mínimo de 10 caracteres, ao menos uma letra maiúscula, uma minúscula, um número e um caractere não alfanumérico. Todas as falhas serão informadas no cadastro.
- Senhas serão armazenadas somente como hash com salt usando `PasswordHasher<TUser>` do ASP.NET Core Identity; senhas em texto puro não serão persistidas, retornadas ou registradas em log.
- A autenticação será JWT Bearer. O token conterá `sub` (identificador do usuário), `email`, `iss`, `aud`, `iat` e `exp`; será assinado com uma chave simétrica fornecida apenas por variável de ambiente. Em desenvolvimento e testes, a chave será definida no Docker Compose e não será incluída em imagens nem código-fonte.
- A validade do access token será configurável por variável de ambiente, com padrão de 60 minutos. Renovação e revogação de token permanecem fora de escopo.
- `POST /auth/register` retornará `201 Created` quando criar a conta. `POST /auth/login` retornará `200 OK` com o access token quando as credenciais forem válidas.
- O CRUD usará `GET /users` para listagem paginada de dados públicos e `GET`, `PUT` e `DELETE /users/{id}` para operações individuais. Todos exigem JWT; as operações individuais são autorizadas somente quando o `sub` corresponde ao id de rota.
- E-mail duplicado retornará `409 Conflict`. Entrada inválida retornará `400 Bad Request` com os campos e regras inválidas. Falha de login, seja por e-mail inexistente ou senha incorreta, retornará sempre `401 Unauthorized` com a mesma mensagem genérica. Recurso inexistente retornará `404 Not Found` e acesso à conta de outro usuário retornará `403 Forbidden`.
- A imagem da API será gerada por Dockerfile multiestágio. O Compose aguardará a saúde do PostgreSQL antes de iniciar a API.
- A API deve gerar um documento OpenAPI e usar Scalar como interface interativa de documentação. O Scalar será disponibilizado na rota `/scalar` no ambiente de desenvolvimento e no Docker local, sem incluir valores de segredos nos exemplos ou na configuração publicada.
- Não serão usados serviços externos. Dependências de compilação serão pacotes versionados do ecossistema .NET restaurados no processo de build; em execução, a solução depende exclusivamente dos contêineres definidos no repositório.

## Modelo de dados

Tabela `users`:

| Coluna | Tipo PostgreSQL | Regras |
|---|---|---|
| `id` | `uuid` | Chave primária; gerada pela aplicação ou banco. |
| `email` | `varchar(320)` | E-mail informado após remoção de espaços; obrigatório. |
| `normalized_email` | `varchar(320)` | E-mail em minúsculas invariantes; obrigatório e único. |
| `password_hash` | `text` | Hash com salt; obrigatório; nunca exposto por contrato HTTP. |
| `created_at_utc` | `timestamp with time zone` | Data/hora de criação em UTC; obrigatória. |

Índices e restrições:

- Chave primária em `id`.
- Restrição única nomeada `ux_users_normalized_email` em `normalized_email`.
- Restrições `NOT NULL` em todas as colunas.
- A restrição única é a proteção definitiva para cadastros simultâneos; sua violação será convertida pela API em `409 Conflict`.

O Compose configurará `tmpfs: /var/lib/postgresql/data`, sem `volumes`, para manter a base de dados integralmente na memória do contêiner. A destruição do contêiner descarta os dados intencionalmente.

## Contratos

### `POST /auth/register`

Corpo da requisição:

```json
{
  "email": "usuario@exemplo.com",
  "password": "SenhaForte@2026"
}
```

Resposta de sucesso (`201 Created`):

```json
{
  "id": "uuid",
  "email": "usuario@exemplo.com"
}
```

Erros: `400 Bad Request` para e-mail ou senha inválidos; `409 Conflict` para e-mail já cadastrado. Respostas não incluem a senha ou seu hash.

### `POST /auth/login`

Corpo da requisição:

```json
{
  "email": "usuario@exemplo.com",
  "password": "SenhaForte@2026"
}
```

Resposta de sucesso (`200 OK`):

```json
{
  "accessToken": "<jwt>",
  "tokenType": "Bearer",
  "expiresIn": 3600
}
```

Falhas de credenciais retornam `401 Unauthorized` com uma mensagem genérica, idêntica para e-mail inexistente e senha incorreta. Campos ausentes ou malformados retornam `400 Bad Request` e não emitem token.

### `GET /users`

Exige `Authorization: Bearer <jwt>`. Aceita `page` e `pageSize` como parâmetros de paginação, com valores positivos e limite máximo definido pela API. Retorna `200 OK` com uma página de usuários contendo somente `id`, `email` e `createdAtUtc`. Senha e hash nunca são retornados.

### `GET /users/{id}`

Exige `Authorization: Bearer <jwt>`. Retorna `200 OK` com `id`, `email` e `createdAtUtc` somente quando o `id` de rota corresponde ao claim `sub` do token. Retorna `403 Forbidden` para outro usuário e `404 Not Found` quando o id não existe.

### `PUT /users/{id}`

Exige `Authorization: Bearer <jwt>` e titularidade da conta. O corpo aceita um ou ambos os campos abaixo:

```json
{
  "email": "novo.usuario@exemplo.com",
  "password": "NovaSenhaForte@2026"
}
```

O e-mail informado é normalizado e precisa ser único; a nova senha precisa obedecer à política de senha forte e é persistida somente como hash. A resposta `200 OK` retorna os dados públicos atualizados. Retorna `400 Bad Request` para corpo inválido, `403 Forbidden` para outro titular, `404 Not Found` para id inexistente e `409 Conflict` para e-mail já utilizado.

### `DELETE /users/{id}`

Exige `Authorization: Bearer <jwt>` e titularidade da conta. Remove o usuário e retorna `204 No Content`; retorna `403 Forbidden` para outro titular e `404 Not Found` quando o id não existe. Após a exclusão, o login com as credenciais removidas deve falhar.

### Documentação interativa

O documento OpenAPI será exposto pela API e o Scalar estará disponível em `/scalar`. A documentação deve declarar o esquema Bearer JWT para os endpoints protegidos, listar todos os contratos de autenticação e CRUD e nunca conter chaves, senhas, hashes ou connection strings reais.

### Testes e operação em Docker

- `docker compose up --build` inicia API e PostgreSQL efêmero.
- `docker compose -f docker-compose.test.yml up --build --abort-on-container-exit` inicia PostgreSQL efêmero e executa a suíte de integração em um contêiner de testes.
- Os testes unitários não dependem de Docker ou banco; devem executar com `dotnet test` dentro do contêiner de testes.
- Os testes de integração devem aplicar as migrações em um banco novo e isolado, criar dados por HTTP e validar cadastro, login, JWT, listagem paginada, consulta, atualização, exclusão, titularidade e persistência sem depender de serviços externos.

## Riscos

- PostgreSQL não possui um modo de banco “em memória” equivalente a SQLite. O uso de `tmpfs` oferece persistência apenas na RAM do contêiner e atende à intenção de efemeridade, mas exige memória disponível no host Docker.
- Uma chave JWT fraca ou versionada no repositório compromete a autenticação. A inicialização deve falhar quando a chave não estiver configurada ou não tiver tamanho seguro; arquivos de segredo local devem ficar no `.gitignore`.
- Tokens JWT são autoportáveis; sem revogação, um token válido continua aceito até expirar. Isso é aceitável no escopo atual, que exclui logout, revogação e renovação.
- Validar unicidade somente na aplicação criaria uma condição de corrida. A restrição única no PostgreSQL é obrigatória e os testes de integração devem cobrir a tradução de sua violação.
- Imagens e pacotes de build precisam ser obtidos na primeira construção. Após a imagem ser gerada, a execução não depende de rede ou sistemas externos.
- Diferenças entre ambiente de desenvolvimento e testes podem mascarar falhas. Ambos devem usar PostgreSQL em Docker e as mesmas migrações da aplicação; não será usado provedor em memória do EF Core para testes de integração.
