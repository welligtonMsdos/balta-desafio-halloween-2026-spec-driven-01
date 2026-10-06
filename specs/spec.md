# Autenticação de Usuários

## Problema

O sistema ainda não possui um mecanismo de identidade para seus usuários. Sem cadastro e autenticação, não é possível identificar quem acessa a aplicação, proteger funcionalidades que exigem uma conta ou preparar o acesso futuro a outros sistemas integrados.

Também é necessário impedir a criação de contas duplicadas e reduzir o risco de senhas fracas. Um mesmo endereço de e-mail cadastrado mais de uma vez gera ambiguidade de identidade, enquanto senhas simples tornam as contas mais vulneráveis a acessos indevidos.

## Objetivo

Disponibilizar um fluxo de cadastro, login e gerenciamento completo da própria conta por e-mail e senha. O usuário deverá conseguir criar uma conta usando um e-mail ainda não registrado e uma senha forte; depois, deverá conseguir autenticar-se, consultar seus dados, atualizar e-mail e/ou senha e excluir a própria conta.

O resultado esperado é uma base de usuários com e-mails únicos e senhas que atendam a uma política mínima de segurança, pronta para suportar futuramente o controle de acesso aos sistemas.

## Usuários

- Visitante: pessoa sem sessão autenticada que deseja criar uma conta para acessar o sistema no futuro.
- Usuário cadastrado: pessoa que já possui uma conta e deseja entrar no sistema com seu e-mail e senha.
- Sistema: responsável por validar os dados recebidos, garantir a unicidade do e-mail, armazenar a senha de forma segura e permitir ou negar a autenticação.

## Histórias

- Como visitante, quero me cadastrar com meu e-mail e uma senha forte para ter uma conta que possa usar para acessar o sistema futuramente.
- Como visitante, quero receber uma mensagem clara quando tento usar um e-mail já cadastrado para saber que preciso usar outro e-mail ou entrar com a conta existente.
- Como visitante, quero ser informado sobre os requisitos da senha para conseguir criar uma senha válida na primeira tentativa.
- Como usuário cadastrado, quero entrar informando meu e-mail e minha senha para acessar o sistema.
- Como usuário cadastrado, quero receber uma mensagem genérica quando as credenciais estiverem incorretas, sem expor se um e-mail específico está cadastrado.
- Como usuário autenticado, quero consultar e atualizar os dados da minha própria conta para mantê-la correta e segura.
- Como usuário autenticado, quero alterar minha senha respeitando a mesma política de senha forte.
- Como usuário autenticado, quero excluir minha própria conta quando não desejar mais utilizá-la.
- Como usuário autenticado, quero listar os dados públicos de usuários de forma paginada, sem acesso a informações sensíveis.

## Requisitos funcionais

- RF-01: o sistema deve disponibilizar um formulário de cadastro contendo os campos obrigatórios e-mail e senha.
- RF-02: o sistema deve validar que o e-mail informado possui formato válido antes de concluir o cadastro.
- RF-03: o sistema deve verificar se já existe um usuário com o mesmo e-mail antes de criar uma nova conta.
- RF-04: o sistema deve recusar o cadastro quando o e-mail já estiver registrado e apresentar uma mensagem informando que ele já está em uso.
- RF-05: o sistema deve validar a força da senha conforme as regras de negócio definidas nesta especificação.
- RF-06: o sistema deve criar o usuário somente quando e-mail e senha forem válidos e o e-mail não existir na base.
- RF-07: o sistema deve armazenar a senha de modo seguro, sem persistir ou expor seu valor em texto puro.
- RF-08: o sistema deve disponibilizar um formulário de login com os campos obrigatórios e-mail e senha.
- RF-09: o sistema deve autenticar o usuário quando o e-mail existir e a senha informada corresponder à senha cadastrada.
- RF-10: o sistema deve negar o login quando o e-mail não existir ou a senha estiver incorreta, apresentando uma mensagem genérica de credenciais inválidas.
- RF-11: após um login bem-sucedido, o sistema deve estabelecer uma sessão autenticada para o usuário, permitindo que o acesso seja reconhecido nas funcionalidades futuras.
- RF-12: o sistema deve permitir que um usuário autenticado consulte os dados públicos de sua própria conta pelo identificador.
- RF-13: o sistema deve permitir que usuários autenticados listem dados públicos de usuários de forma paginada.
- RF-14: o sistema deve permitir que o titular autenticado atualize o e-mail e/ou a senha de sua própria conta.
- RF-15: o sistema deve validar novamente a unicidade do e-mail e a política de senha forte sempre que esses dados forem alterados.
- RF-16: o sistema deve permitir que o titular autenticado exclua sua própria conta.

## Regras de negócio

- RN-01: o e-mail é o identificador único do usuário. Não pode haver mais de uma conta com o mesmo e-mail.
- RN-02: a comparação de unicidade do e-mail deve desconsiderar diferenças entre letras maiúsculas e minúsculas. Por exemplo, `usuario@exemplo.com` e `USUARIO@EXEMPLO.COM` representam o mesmo e-mail.
- RN-03: a senha deve possuir no mínimo 10 caracteres.
- RN-04: a senha deve conter ao menos uma letra maiúscula.
- RN-05: a senha deve conter ao menos uma letra minúscula.
- RN-06: a senha deve conter ao menos um número.
- RN-07: a senha deve conter ao menos um caractere especial, como `!`, `@`, `#`, `$`, `%`, `&`, `*`, `-`, `_` ou outro caractere não alfanumérico aceito pela aplicação.
- RN-08: todas as regras de senha devem ser atendidas simultaneamente para que o cadastro seja concluído.
- RN-09: a senha nunca deve ser retornada por telas, APIs, registros de auditoria ou logs.
- RN-10: uma tentativa de login não deve revelar se o e-mail existe; para e-mail inexistente ou senha incorreta, a resposta deve ser a mesma: credenciais inválidas.
- RN-11: os endpoints de CRUD exigem um JWT válido. O identificador do usuário no claim `sub` deve corresponder ao identificador solicitado para consultar, atualizar ou excluir uma conta individual.
- RN-12: a listagem de usuários é permitida apenas a usuários autenticados, deve ser paginada e pode retornar somente dados públicos, nunca senha ou hash.
- RN-13: uma alteração de senha deve atender a todas as regras RN-03 a RN-08 e gerar um novo hash antes da persistência.
- RN-14: a exclusão da conta remove o usuário e deve impedir seu acesso subsequente com as credenciais removidas.

## Casos de borda

- Cadastro com e-mail vazio, apenas espaços ou formato inválido deve ser recusado e informar que um e-mail válido é obrigatório.
- Cadastro com senha vazia, apenas espaços ou com menos de 10 caracteres deve ser recusado e indicar o requisito não atendido.
- Senhas que atendam ao tamanho mínimo, mas não possuam maiúscula, minúscula, número ou caractere especial devem ser recusadas, indicando os critérios ausentes.
- Um e-mail já registrado com capitalização diferente deve ser considerado duplicado e não pode gerar uma nova conta.
- Espaços não intencionais antes ou depois do e-mail devem ser ignorados para fins de validação e unicidade.
- O login com e-mail ou senha em branco deve ser recusado sem iniciar uma sessão.
- O login com senha válida, mas pertencente a outro usuário, deve falhar com a mesma mensagem de credenciais inválidas.
- Duas tentativas simultâneas de cadastro usando o mesmo e-mail devem resultar em apenas uma conta criada; a outra tentativa deve receber a indicação de e-mail já cadastrado.
- A consulta, atualização ou exclusão de uma conta por um usuário autenticado diferente do titular deve ser negada sem alterar dados.
- A atualização para um e-mail já registrado, inclusive com capitalização diferente, deve ser recusada.
- A atualização de senha que não atenda à política de senha forte deve ser recusada e não pode alterar o hash existente.
- A consulta, atualização ou exclusão de um identificador inexistente deve retornar que o recurso não foi encontrado.
- A listagem deve rejeitar paginação inválida e não pode incluir senha ou hash em nenhum item retornado.

## Fora de escopo

- Recuperação ou redefinição de senha por fluxo externo ao usuário autenticado.
- Confirmação ou verificação de propriedade do e-mail.
- Login por provedores externos, como Google, Microsoft ou redes sociais.
- Autenticação em dois fatores (2FA/MFA).
- Perfis, papéis e permissões administrativas; a autorização mínima por titularidade da própria conta é parte do escopo.
- Bloqueio de conta, limitação de tentativas de login ou mecanismos anti-bot.
- Integração com outros sistemas; esta especificação prepara a base de usuários para essas integrações futuras.

## Critérios de aceite

- Dado que informo um e-mail válido e ainda não cadastrado e uma senha com pelo menos 10 caracteres, incluindo letra maiúscula, letra minúscula, número e caractere especial, quando concluo o cadastro, então o sistema cria a conta e confirma o sucesso.
- Dado que já existe uma conta com um e-mail, quando tento me cadastrar novamente com esse mesmo e-mail, inclusive usando letras maiúsculas ou minúsculas diferentes, então o sistema não cria outra conta e informa que o e-mail já está em uso.
- Dado que informo uma senha com menos de 10 caracteres ou sem qualquer uma das quatro categorias exigidas, quando tento concluir o cadastro, então o sistema não cria a conta e informa os requisitos de senha não atendidos.
- Dado que possuo uma conta cadastrada, quando informo no login o e-mail correspondente e a senha correta, então o sistema me autentica e estabelece uma sessão.
- Dado que informo um e-mail inexistente ou uma senha incorreta, quando tento fazer login, então o sistema não estabelece uma sessão e retorna uma mensagem genérica de credenciais inválidas.
- Dado que uma senha é cadastrada, quando os dados do usuário são persistidos ou registrados, então a senha não está disponível em texto puro nem é exibida em respostas, telas ou logs.
- Dado que estou autenticado, quando consulto minha própria conta, então recebo seus dados públicos e nunca a senha ou seu hash.
- Dado que estou autenticado, quando listo usuários com parâmetros de paginação válidos, então recebo uma página de dados públicos sem senhas ou hashes.
- Dado que estou autenticado como titular de uma conta, quando atualizo seu e-mail para um valor válido e único e/ou sua senha para um valor forte, então os dados são alterados; quando uso e-mail duplicado ou senha inválida, então nenhuma alteração é persistida.
- Dado que estou autenticado como titular de uma conta, quando solicito sua exclusão, então a conta é removida e um novo login com suas credenciais falha.
- Dado que estou autenticado como outro usuário, quando tento consultar, atualizar ou excluir uma conta que não é minha, então a operação é negada.
