<img width="100%" alt="Halloween 2026" src="https://baltaio.blob.core.windows.net/static/images/v4/challenges/halloween-2026/banner.jpg" />

## 🎃 Halloween - Desafio 1

Oi, eu sou o Welligton Silva e este é o espaço onde compartilho minha jornada de aprendizado durante o desafio **Halloween 2026**, realizado pelo [balta.io](https://balta.io). 👻

Aqui você vai encontrar projetos, exercícios e códigos que estou desenvolvendo durante o desafio.

### Sobre este desafio
Neste desafio o objetivo é consolidar os fundamentos do Spec Driven Development, criando uma constituição, especificação, planejamento e tarefas manualmente para serem implementadas pela IA posteriormente.

#### O que foi implementado
- API Minimal em C# com .NET 10 para cadastro, login e gerenciamento de usuários.
- Autenticação JWT Bearer e proteção das operações individuais para que cada usuário acesse somente a própria conta.
- CRUD de usuários: cadastro, consulta individual, listagem paginada, atualização de e-mail/senha e exclusão.
- Validação de e-mail único, comparação sem distinção entre maiúsculas e minúsculas e senha forte com pelo menos 10 caracteres, incluindo maiúscula, minúscula, número e caractere especial.
- Armazenamento de senhas com hash e salt; senhas e hashes não são retornados pela API.
- PostgreSQL executado em Docker com armazenamento efêmero em `tmpfs`, além de migrações do Entity Framework Core e repositório seguindo Repository Pattern.
- Documentação interativa da API com OpenAPI e Scalar em `/scalar` no ambiente de desenvolvimento.
- Testes unitários e de integração, com execução local por `dotnet test Auth.slnx` ou no Docker Compose de testes.
- Dockerfile e Docker Compose para executar a API e o banco de dados.

Neste processo eu aprendi:
* ✅ Uma especificação mais detalhada deixa mais claro como elaborar o plano para executá-la.
* ✅ Com a especificação e o plano bem definidos, as tarefas ficam mais claras e mostram melhor o que será produzido.
* ✅ A IA executou corretamente todas as tarefas, criando um commit específico para cada uma e sem fazer nada além do que foi pedido.
* ✅ Para mim, essa experiência foi surreal e incrível.

## Bagde
<img src="https://baltaio.blob.core.windows.net/static/images/v4/challenges/halloween-2026/01.png" width="200" />

## Problema
--

## Sobre o Halloween 2026
O desafio **Halloween 2026** consiste em implementar implementar o modelo Spec Driven Development de ponta a ponta, criando apps completas com IA.

### Veja meu progresso no desafio
[https://github.com/welligtonMsdos/balta-desafio-halloween-2026-spec-driven-01]

## Como executar a API

Você pode iniciar a API e o banco de dados juntos usando o Docker Compose. Antes de começar, confirme que o Docker Desktop está aberto e em execução.

### 1. Abra o terminal na pasta do projeto

Navegue até a pasta raiz do repositório, onde está o arquivo `docker-compose.yml`.

### 2. Inicie a API e o banco de dados

Execute:

```powershell
docker compose up --build --detach
```

O Docker irá construir a imagem da API, iniciar a API e aguardar o PostgreSQL ficar pronto. O banco usa armazenamento temporário; os dados são descartados quando o contêiner do banco é recriado.

### 3. Confira se os serviços estão em execução

```powershell
docker compose ps
```

Quando os serviços estiverem prontos, a API poderá ser acessada localmente.

### 4. Abra a documentação interativa

Acesse no navegador:

**[Abrir a API no Scalar](http://localhost:8080/scalar/v1)**

Na página do Scalar você pode consultar os endpoints e experimentar as requisições da API.

### Parar a API

Quando terminar, pare e remova os contêineres com:

```powershell
docker compose down
```
