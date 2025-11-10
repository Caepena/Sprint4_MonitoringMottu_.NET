# 🏍️ Sprint 4 – Sistema de Gestão de Motos e Garagens

Este é um projeto em **.NET 8** que implementa uma **API RESTful** para gerenciamento de **motos** e **garagens**, com foco em **boas práticas REST**, **camadas bem definidas**, e integração com **Oracle Database** via **Entity Framework Core**.

---

## 📦 Estrutura de Projeto

```
MonitoringMottu.API/              → Camada de apresentação (Controllers, Program, Swagger)
MonitoringMottu.Application/      → Casos de uso e regras de negócio
MonitoringMottu.Domain/           → Entidades e interfaces
MonitoringMottu.Infrastructure/   → Repositórios e contexto do EF Core (Oracle)
MonitoringMottuCP4.API.Tests/     → Testes automatizados (xUnit + Moq)
```

---

## 📡 Endpoints Disponíveis

### 🚗 **MotoController**

| Método | Rota | Descrição |
|:-------|:------|:-----------|
| `GET` | `/moto` | Listar todas as motos |
| `GET` | `/moto/{id}` | Buscar moto por ID |
| `POST` | `/moto` | Cadastrar nova moto |
| `PUT` | `/moto/{id}` | Atualizar moto existente |
| `DELETE` | `/moto/{id}` | Excluir moto |

### 🏢 **GaragemController**

| Método | Rota | Descrição |
|:-------|:------|:-----------|
| `GET` | `/garagem` | Listar todas as garagens |
| `GET` | `/garagem/{id}` | Buscar garagem por ID |
| `POST` | `/garagem` | Cadastrar nova garagem |
| `PUT` | `/garagem/{id}` | Atualizar garagem existente |
| `DELETE` | `/garagem/{id}` | Excluir garagem |

---

## 🩺 **Health Check**

O endpoint `/health-check` foi adicionado para verificar a integridade do sistema e do banco Oracle.

- **Rota:** `GET /health-check`  
- **Retorno:** JSON com status da aplicação e do banco de dados.

Exemplo de resposta:

```json
{
  "status": "Healthy",
  "duration": "00:00:00.053241",
  "info": [
    {
      "key": "self",
      "status": "Healthy"
    },
    {
      "key": "OracleDB",
      "status": "Healthy"
    }
  ]
}
```

---

## 🧪 Testes Automatizados

Os testes foram desenvolvidos com **xUnit** e **Moq**, garantindo a qualidade das principais funcionalidades.

### 📁 Estrutura de Testes

| Tipo de Teste | Descrição |
|----------------|------------|
| **Unitários** | Testam casos de uso e controllers isoladamente |
| **Integração** | Validam rotas reais via `WebApplicationFactory` (ex: HealthCheck e MotoController) |

### ▶️ Como rodar os testes

Abra o terminal na raiz da solução e execute:

```bash
dotnet test
```

> 💡 Os testes rodam sobre o banco em memória (mockado com Moq), não exigem Oracle em execução.

Os resultados serão exibidos assim:

```
Aprovado: 16, Com falha: 0, Ignorado: 0, Total: 16, Duração: 47 ms
```

---

## ⚙️ Configuração e Execução da API

### 1️⃣ **Pré-requisitos**
- .NET 8 SDK instalado  
- Banco Oracle acessível (por exemplo: `oracle.fiap.com.br:1521/orcl`)

### 2️⃣ **Configurar conexão**
No arquivo `appsettings.json`:

```json
"ConnectionStrings": {
  "OracleConnection": "User ID=rm557984;Password=191101;Data Source=oracle.fiap.com.br:1521/orcl;"
}
```

### 3️⃣ **Executar o projeto**
```bash
dotnet run --project src/MonitoringMottu.API
```

A aplicação rodará por padrão em:

- Swagger: http://localhost:5183/swagger  
- Health Check: http://localhost:5183/health-check

---

## 🧰 Tecnologias Utilizadas

- **.NET 8 / ASP.NET Core**
- **Entity Framework Core (Oracle)**
- **xUnit + Moq**
- **HealthChecks (Xabaril)**
- **Swagger / OpenAPI**
- **Oracle Managed Data Access**

---

## 📌 Funcionalidades Implementadas

- ✅ Cadastro, listagem e exclusão de garagens  
- ✅ Cadastro, atualização e listagem de motos  
- ✅ Associação entre motos e garagens  
- ✅ Persistência com **Oracle + EF Core**  
- ✅ **Health Check** (`/health-check`)  
- ✅ **Testes automatizados** (xUnit + Moq)

---

## 👥 Integrantes

| Nome | RM |
|------|----|
| **Caetano Penafiel Matos** | RM557984 |
| **Kauã Fermino Zipf** | RM558957 |
| **Victor Egídio Lira** | RM556653 |

---

## 🧩 Observações Finais

- A string de conexão foi definida diretamente no `appsettings.json` por simplicidade de entrega.  
- Todos os endpoints REST seguem convenções HTTP e boas práticas RESTful.  
