# 🏦 BankMore API - Sistema de Banco Digital

Este projeto é uma API robusta para um banco digital, desenvolvida em **.NET 9** seguindo os padrões **DDD**, **Arquitetura Hexagonal**, **CQRS** e **Clean Code**. A persistência é feita no **Oracle Database** via **Dapper**.

---

## 🛠️ 1. Configuração do Banco de Dados (Oracle SQL Developer)

Para criar a base de dados, siga este passo a passo no seu **Oracle SQL Developer**:

1.  **Conexão:** Abra sua conexão `BankMoreDb` (User: `system`, Host: `localhost`, Port: `1521`, SID: `xe`).
2.  **Abrir Script:** Localize o arquivo `oracle_migrations.sql` na raiz do projeto.
3.  **Executar:** Copie todo o conteúdo do arquivo, cole na planilha SQL do SQL Developer e pressione **F5** (Executar Script).
    *   *Nota:* O script limpa tabelas antigas e cria a nova estrutura (Usuário, Agência, Conta Corrente, Movimentos, etc.).
4.  **Verificar:** Certifique-se de que a mensagem "Commit concluído" apareça no final.

---

## 🔐 2. Fluxo de Autenticação e Tokens (JWT)

A API utiliza dois níveis de segurança para garantir que as operações bancárias sejam feitas na conta correta.

### Passo 1: Login do Usuário
*   **Endpoint:** `POST /api/auth/login`
*   **Payload:** `{ "cpf": "12345678901", "senha": "sua_senha" }`
*   **O que você recebe:** Um `Token` de acesso ao perfil do usuário.
*   **Como usar:** No Swagger, clique em **Authorize** e digite: `Bearer [seu_token]`.

### Passo 2: Selecionar Conta (Token Operacional)
Como um usuário pode ter várias contas, você precisa dizer à API qual conta deseja operar (Saldo, Depósito, Saque, Transferência).
*   **Endpoint:** `POST /api/auth/selecionar-conta`
*   **Payload:** `123456` (Número da conta que você criou).
*   **O que você recebe:** Um `TokenOperacional`.
*   **Como usar:** **IMPORTANTE!** Você deve substituir o token anterior no botão **Authorize** do Swagger pelo novo `TokenOperacional`. Digite: `Bearer [token_operacional]`.

---

## 🚀 3. Guia de Teste Completo (Ordem Recomendada)

Siga esta ordem para testar todas as funcionalidades:

1.  **Cadastrar Agência:** `POST /api/agencia/cadastrar` (Ex: Numero: "001", Nome: "Agência Central"). Guarde o `idAgencia`.
2.  **Cadastrar Usuário:** `POST /api/usuario/cadastrar` (Dados completos).
3.  **Login:** `POST /api/auth/login` (Obtenha o Token de Usuário).
4.  **Abrir Conta:** `POST /api/contacorrente/cadastrar` (Use o Token de Usuário e o `idAgencia`). Guarde o `numeroConta`.
5.  **Selecionar Conta:** `POST /api/auth/selecionar-conta` (Use o Token de Usuário e o `numeroConta`). **Obtenha o Token Operacional**.
6.  **Operações Bancárias:** Use o **Token Operacional** para:
    *   `POST /api/contacorrente/deposito` (Adicione saldo).
    *   `GET /api/contacorrente/saldo` (Verifique o saldo).
    *   `POST /api/contacorrente/saque` (Retire valores).
    *   `POST /api/transferencia` (Transfira entre contas da mesma agência).

---

## 🏗️ Arquitetura e Tecnologias
*   **.NET 9** com C#
*   **Dapper** (Micro-ORM para performance)
*   **MediatR** (Implementação de CQRS)
*   **Oracle.ManagedDataAccess** (Conexão nativa Oracle)
*   **Swagger/OpenAPI** (Documentação interativa)
*   **KafkaFlow** (Mensageria para tarifação assíncrona)


