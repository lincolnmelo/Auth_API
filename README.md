# API de Autenticação com .NET 8 + Minimal API + MariaDB

Esta é uma API de autenticação completa usando .NET 8 com Minimal API e MariaDB, criada com migrations.

## Funcionalidades

- Registro de usuários
- Login de usuários
- Autenticação via JWT (simplificada para demonstração)
- Persistência em MariaDB usando Entity Framework Core
- Migrations para criação da estrutura do banco de dados

## Estrutura do Projeto

```
AuthAPI/
├── Models/
│   ├── User.cs
│   ├── LoginRequest.cs
│   ├── LoginResponse.cs
│   └── RegisterRequest.cs
├── Services/
│   ├── IAuthService.cs
│   └── AuthService.cs
├── Controllers/
│   └── AuthController.cs
├── Migrations/
└── appsettings.json
```

## Tecnologias Utilizadas

- .NET 8
- Minimal API
- MariaDB (via Pomelo.EntityFrameworkCore.MySql)
- Entity Framework Core
- Swagger/OpenAPI

## Configuração

1. **String de Conexão**: Atualize a string de conexão no `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=auth_api_db;Uid=root;Pwd=sua_senha;SslMode=none;"
     }
   }
   ```

2. **Executar migrações** (se necessário):
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

## Endpoints

### Registro de Usuário
```
POST /api/auth/register
{
  "username": "usuario",
  "email": "usuario@example.com",
  "password": "senha123"
}
```

### Login
```
POST /api/auth/login
{
  "username": "usuario",
  "password": "senha123"
}
```

## Como Executar

1. Restaure as dependências:
   ```bash
   dotnet restore
   ```

2. Execute a aplicação:
   ```bash
   dotnet run
   ```

3. Acesse a documentação Swagger em: `https://localhost:5001/swagger`

## Considerações de Segurança

- Em produção, use bibliotecas como `System.IdentityModel.Tokens.Jwt` para geração de tokens JWT
- Use algoritmos de hash mais seguros como BCrypt ou Argon2 para senhas
- Implemente validações adicionais e tratamento de exceções
- Configure HTTPS em produção

## Próximos Passos

1. Adicionar middleware de autenticação
2. Implementar refresh tokens
3. Adicionar validações mais robustas
4. Configurar logging
5. Adicionar testes unitários