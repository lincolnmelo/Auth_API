# API de Autenticação com .NET 8 + Minimal API + MariaDB

Esta é uma API de autenticação completa usando .NET 8 com Minimal API e MariaDB, criada com migrations.

## Funcionalidades

- Registro de usuários
- Login de usuários
- Autenticação via JWT (com implementação completa)
- Persistência em MariaDB usando Entity Framework Core
- Migrations para criação da estrutura do banco de dados
- Validações de dados e tratamento de exceções
- Segurança com hash de senhas e tokens JWT

## Estrutura do Projeto

```
AuthAPI/
├── Models/
│   ├── Entities/
│   │   └── User.cs
│   ├── AuthContext.cs
│   ├── LoginRequest.cs
│   ├── LoginResponse.cs
│   └── RegisterRequest.cs
├── Services/
│   ├── IAuthService.cs
│   └── AuthService.cs
├── Features/
│   └── Auth/
│       ├── DTOs/
│       │   ├── LoginRequest.cs
│       │   ├── LoginResponse.cs
│       │   └── RegisterRequest.cs
│       └── Endpoints/
│           └── AuthEndpoints.cs
├── Infrastructure/
│   └── Data/
│       └── AuthContext.cs
├── Properties/
│   └── launchSettings.json
├── Migrations/
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

## Tecnologias Utilizadas

- .NET 8
- Minimal API
- MariaDB (via Pomelo.EntityFrameworkCore.MySql)
- Entity Framework Core
- Swagger/OpenAPI
- JWT Authentication
- SHA256 para hash de senhas

## Configuração

1. **String de Conexão**: Atualize a string de conexão no `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=auth_api_db;Uid=root;Pwd=sua_senha;SslMode=none;"
     },
     "JwtSettings": {
       "SecretKey": "sua_chave_secreta_segura_aqui",
       "Issuer": "AuthAPI",
       "Audience": "AuthAPIUsers",
       "ExpireInMinutes": 60
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
POST /auth/register
{
  "username": "usuario",
  "email": "usuario@example.com",
  "password": "senha123"
}
```

### Login
```
POST /auth/login
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
- Use algoritmos de hash mais seguros como BCrypt ou Argon2 para senhas (atualmente implementado com SHA256)
- Implemente validações adicionais e tratamento de exceções
- Configure HTTPS em produção
- Mantenha a chave secreta JWT segura e não exposta

## Próximos Passos

1. Adicionar middleware de autenticação
2. Implementar refresh tokens
3. Adicionar validações mais robustas
4. Configurar logging
5. Adicionar testes unitários
6. Melhorar a segurança com BCrypt para hash de senhas
7. Implementar políticas de autorização
8. Adicionar documentação mais detalhada dos endpoints