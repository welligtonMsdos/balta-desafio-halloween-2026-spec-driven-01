# Constituição do Projeto — Autenticação e CRUD de Usuários

## Stack

- Linguagem e runtime: C# sobre .NET 10, com nullable reference types habilitado.
- API: ASP.NET Core Minimal API para endpoints HTTP, autenticação JWT Bearer e tratamento centralizado de erros.
- Persistência: Entity Framework Core com o provedor Npgsql para PostgreSQL.
- Segurança: `PasswordHasher<TUser>` do ASP.NET Core Identity para hash de senha com salt e JWT assinado com chave simétrica configurada por ambiente.
- Banco de dados: PostgreSQL em contêiner Docker. O diretório `/var/lib/postgresql/data` deve usar `tmpfs`, sem volume persistente, tornando os dados efêmeros e mantidos na memória do contêiner.
- Contêineres: Dockerfile multiestágio para a API, Docker Compose para desenvolvimento e Docker Compose próprio para execução de testes.
- Testes: suíte de testes unitários e de integração em .NET; os testes de integração usam PostgreSQL real em Docker, nunca o provedor em memória do EF Core.
- Dependências externas: a aplicação em execução não pode chamar APIs, serviços de identidade, bancos ou recursos externos. Pacotes de compilação devem ser versionados e restaurados somente durante o build.

## Arquitetura

- A solução deve conter os projetos `Auth.Api`, `Auth.Application`, `Auth.Domain`, `Auth.Infrastructure`, `Auth.UnitTests` e `Auth.IntegrationTests`.
- `Auth.Domain` contém a entidade `User`, suas invariantes e erros de domínio. Não referencia ASP.NET Core, EF Core, Npgsql ou infraestrutura.
- `Auth.Application` contém casos de uso, DTOs e interfaces para repositório, hash de senha, relógio e emissão de token. Não depende de HTTP nem de EF Core.
- `Auth.Infrastructure` implementa as interfaces da aplicação usando EF Core, Npgsql, `PasswordHasher<TUser>` e serviços JWT. Contém `DbContext`, mapeamentos e migrações.
- `Auth.Api` compõe as dependências, expõe endpoints Minimal API, configura JWT Bearer e transforma resultados e erros de aplicação em respostas HTTP.
- O projeto adota obrigatoriamente o Repository Pattern: cada agregado persistido possui uma interface de repositório na camada de aplicação/domínio e uma implementação na infraestrutura. `DbContext`, EF Core e consultas SQL não podem ser acessados por API, serviços de aplicação ou domínio.
- Toda regra de orquestração de caso de uso deve estar em um serviço de aplicação com responsabilidade única, como `UserService` e `AuthService`. Endpoints apenas validam o contrato HTTP, delegam ao serviço e convertem o resultado em resposta.
- Serviços dependem de abstrações injetadas por dependência, nunca de implementações concretas de repositório, `DbContext` ou detalhes de infraestrutura. Repositórios tratam exclusivamente da persistência e não contêm regras de autenticação ou HTTP.
- O fluxo de cadastro deve normalizar e validar dados, verificar unicidade, gerar hash e persistir em transação. O fluxo de login deve buscar pelo e-mail normalizado, verificar o hash e emitir o JWT.
- A tabela `users` deve possuir `id` UUID, `email`, `normalized_email`, `password_hash` e `created_at_utc`, todos obrigatórios. `normalized_email` deve ter a restrição única `ux_users_normalized_email`.
- O banco é a autoridade final de unicidade. Erros da restrição única devem ser convertidos em conflito de negócio, inclusive sob concorrência.
- O CRUD deve ser protegido por JWT: usuários autenticados podem listar somente dados públicos; consulta, atualização e exclusão individuais são permitidas somente ao titular cujo id corresponda ao claim `sub` do token.

## Qualidade

- Todo comportamento novo deve ter testes unitários das regras de domínio e aplicação e testes de integração dos endpoints e persistência.
- Testes unitários devem cobrir: normalização e formato de e-mail, todos os critérios de senha forte, cadastro, conflito, login válido e inválido, emissão de token e operações CRUD.
- Testes de integração devem cobrir: migrações, cadastro válido, e-mail duplicado inclusive com capitalização diferente, regras de senha, login e JWT, CRUD completo, paginação, autorização por proprietário e tentativas de acesso por terceiro.
- Testes de segurança devem assegurar que tokens adulterados ou expirados são recusados e que respostas, exceções e logs não expõem senha, hash ou existência de e-mail durante falha de login.
- O código deve compilar sem erros, manter análise estática e nullable reference types ativos e usar dependências com versões explícitas e reprodutíveis.
- A validação final obrigatória é executada no Docker: build da imagem, PostgreSQL efêmero, migrações, testes unitários e testes de integração devem concluir com sucesso.
- Serviços e repositórios devem ter testes unitários próprios: serviços com repositórios substitutos e implementações de repositório validadas por testes de integração contra PostgreSQL.

## Convenções

- E-mail é obrigatório, tem espaços externos removidos e é normalizado com minúsculas invariantes para consulta e unicidade. A representação normalizada não substitui o valor público informado do e-mail.
- Senha forte exige simultaneamente: mínimo de 10 caracteres, uma letra maiúscula, uma minúscula, um número e um caractere não alfanumérico.
- Senhas nunca podem ser persistidas, retornadas, serializadas ou registradas em texto puro. Apenas o hash com salt é armazenado.
- O JWT deve conter os claims `sub`, `email`, `iss`, `aud`, `iat` e `exp`; emissor, audiência, chave e validade são configurações de ambiente. A inicialização deve falhar se a chave estiver ausente ou insegura.
- O access token tem validade padrão de 60 minutos. Renovação, revogação, logout, MFA e provedores externos não fazem parte da implementação atual.
- Endpoints de autenticação: `POST /auth/register` retorna `201 Created`; `POST /auth/login` retorna `200 OK` com `accessToken`, `tokenType` e `expiresIn`.
- Endpoints do CRUD: `GET /users`, `GET /users/{id}`, `PUT /users/{id}` e `DELETE /users/{id}` exigem Bearer token. A listagem deve ser paginada e todos os contratos retornam somente campos públicos.
- Status HTTP: dados inválidos retornam `400 Bad Request`; e-mail duplicado retorna `409 Conflict`; credenciais inválidas retornam `401 Unauthorized` com mensagem idêntica para e-mail inexistente ou senha incorreta; acesso a recurso de outro usuário retorna `403 Forbidden`; recurso inexistente retorna `404 Not Found`; exclusão bem-sucedida retorna `204 No Content`.
- Datas devem ser armazenadas e manipuladas em UTC. Identificadores devem ser UUIDs.
- O PostgreSQL em Docker deve declarar healthcheck e `tmpfs: /var/lib/postgresql/data`; não é permitido configurar volume persistente para a base efêmera.
- Segredos não podem ser commitados, incluídos na imagem Docker ou escritos em logs. Arquivos locais de ambiente devem estar no `.gitignore`.
- Código limpo é obrigatório: nomes devem expressar intenção; métodos e classes devem ter responsabilidade única; duplicação deve ser extraída quando representar a mesma regra; dependências devem apontar para abstrações; e código morto, comentários redundantes e lógica de negócio em endpoints são proibidos.
- Interfaces de repositório devem usar nomes de intenção de domínio, como `GetByIdAsync`, `GetByNormalizedEmailAsync`, `ListAsync`, `AddAsync`, `UpdateAsync` e `DeleteAsync`. Serviços são os únicos responsáveis por coordenar essas operações para cumprir um caso de uso.

## Governança

- `specs/spec.md` define necessidades e regras de negócio; `specs/plan.md` define a solução técnica; `specs/tasks.md` é a fonte de execução ordenada. Esta constituição estabelece limites e padrões obrigatórios para todos eles.
- Quando houver conflito entre os documentos, a solicitação mais recente do usuário prevalece. O pedido de CRUD completo foi incorporado pela T-01 em `spec.md`, `plan.md`, `tasks.md` e nesta constituição; alterações futuras devem preservar essa consistência documental.
- Nenhuma tarefa pode ser considerada concluída sem atender aos seus critérios de aceite e sem os testes aplicáveis passando.
- Alterações de schema devem ser feitas somente por migrações versionadas e revisadas; alterações manuais no banco não são parte do processo.
- Alterações em contratos HTTP, regras de autenticação, política de senha, autorização ou modelo de dados exigem atualização coordenada de especificação, plano, tarefas, testes e documentação de uso.
- Uma revisão não pode aprovar acesso direto a `DbContext` fora da infraestrutura, regra de negócio em endpoint ou uso de persistência sem repositório e serviço correspondente. Exceções exigem atualização explícita desta constituição antes da implementação.
- O escopo não deve crescer silenciosamente. Funcionalidades como recuperação de senha, confirmação de e-mail, autenticação social, MFA, papéis administrativos, limitação de tentativas, revogação de token ou integração externa exigem nova especificação, plano e tarefas antes de implementação.
- A conclusão do projeto exige executar a validação Docker definida na T-38 e documentar os comandos necessários para iniciar, testar e encerrar o ambiente efêmero.
