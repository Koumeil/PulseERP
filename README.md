[![Build Status](https://github.com/Kouumss/PulseERP/actions/workflows/ci.yml/badge.svg)](https://github.com/Kouumss/PulseERP/actions)  
[![License](https://img.shields.io/badge/License-MIT-yellow)](LICENSE)
[![.NET Version](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com)  

TEST////////////////////////////

content = """[![Build Status](https://img.shields.io/github/actions/workflow/status/your-org/PulseERP/ci.yml?branch=main)](https://github.com/your-org/PulseERP/actions)  
[![Coverage](https://img.shields.io/codecov/c/github/your-org/PulseERP/main)](https://codecov.io/gh/your-org/PulseERP)  
[![Docker Pulls](https://img.shields.io/docker/pulls/your-dockerhub/pulseerp)](https://hub.docker.com/r/your-dockerhub/pulseerp)  
[![NuGet](https://img.shields.io/nuget/v/PulseERP)](https://www.nuget.org/packages/PulseERP)  
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE.md)  

# PulseERP
>**🚀 PulseERP** - Mini ERP évolutif pour petites entreprises
>
PulseERP est une solution logicielle spécialement pensée pour répondre aux besoins essentiels des petites entreprises. Conçu pour simplifier la gestion quotidienne des clients, employés, produits, et inventaires, monitoring, PulseERP offre une expérience intuitive, robuste et évolutive.

Construit autour des meilleures pratiques modernes telles que la **Clean Architecture** & **Domain-Drive-Design**. (DDD), ce projet est en constante amélioration et bénéficie d'une conception modulaire facilitant l’ajout futur d’un front-end Angular ou Blazor pour une expérience utilisateur complète et immersive. Inclus une pipeline CI GitHub Actions qui construit, analyse et valide automatiquement.

---

## 🏗️ Vue d’ensemble du dépôt

## 🌟 Fonctionnalités clés

- 🏗️ **Clean Architecture** strictement découplée (Domain, Application, Infrastructure, API)  
- 🚀 **Performances optimisées** (Entity Framework Core + Redis pour le cache)  
- 🔒 **Sécurité renforcée** (JWT avec Refresh Tokens, FluentValidation)  
- 📊 **Gestion complète** des produits, marques, clients, utilisateurs, Inventaire 
- 📈 **Pagination avancée** et filtres dynamiques sur toutes les ressources
- 📈 **MediaTR-Notification** : EventHandler dédié pour chaque fonctionnalité, avec service d'envoi d’e-mails via SMTP (ServiceEmail) 
- 🧪 **Tests unitaires & d’intégration** (xUnit + FluentAssertions)
- 📦 **CI/CD** avec GitHub Actions pour build, tests et couverture  
z
---

## 📂 Structure du projet (arborescence)

```text
PulseERP/
├── .github/                     
│   └── workflows/             
│       └── ci.yml                   # Pipeline CI/CD (build, tests, couverture)
├── docker-compose.yml               # Orchestration : Redis, SQL Server, API
├── PulseERP.sln                     # Solution globale
│
├── PulseERP.Domain/                 # 🌐 Domaine métier (entités, VOs, exceptions)
│   ├── Entities/                    # — Product, User, Brand, Customer, RefreshToken …
│   ├── ValueObjects/                # — Money, EmailAddress, Role, Password, Phone …
│   └── Errors/                      # — DomainException, NotFoundException
│
├── PulseERP.Abstractions/           # 🔌 Ports & DTOs transverses
│   ├── Common/                      # — Pagination, Filters, …
│   └── Security/                    # — Auth DTO, Token, interfaces (IEmailSender, …)
│
├── PulseERP.Application/            # ⚙️ Cas d’usage, services, mapping AutoMapper
│   ├── Products/                    # — Commands, Models, Services
│   ├── Customers/                   # — Commands, Models, Services
│   ├── Mapping/                     # — Profils AutoMapper
│   └── DependencyInjection.cs       # — Enregistrement dans IServiceCollection
│
├── PulseERP.Infrastructure/         # 🛠️ Implémentations techniques
│   ├── Data/                        # — DbContext EF Core + Migrations
│   ├── Repositories/                # — Repos EF Core (avec cache Redis)
│   ├── Providers/                   # — DateTimeProvider, TokenHasher, …
│   └── Smtp/                        # — EmailSender + templates MIME
│
├── PulseERP.API/                    # 🌐 ASP.NET Core 9 – endpoints REST
│   ├── Contracts/                   # — DTOs spécifiques à l’API (ApiResponse, etc.)
│   ├── appsettings*.json            # — Config (valeurs vides pour secrets)
│   ├── Program.cs                   # — Construction du WebApplication
│   └── Dockerfile                   # — Dockerfile pour l’API
│
├── PulseERP.Tests/                  # 🧪 xUnit + FluentAssertions (unitaires & intégration)
└── README.md                        # 📄 Cette documentation

```
---

### PulseERP.Tests

| Paquet                      | Version |
| --------------------------- | ------- |
| coverlet.collector          | 6.0     |
| FluentAssertions            | 8.3     |
| Microsoft.NET.Test.Sdk      | 17.14   |
| xunit                       | 2.9     |
| xunit.runner.visualstudio   | 3.1     |

---

## ⚙️ Prérequis

| Outil                    | Version minimale     | Lien                                         |
| ------------------------ | -------------------- | -------------------------------------------- |
| .NET SDK                 | 9.0.300              | https://dotnet.microsoft.com                 |
| EF Core Tools            | 9.0.\*               | https://docs.microsoft.com/ef                |
| Docker & Docker Compose  | ≥ 24.0               | https://docs.docker.com                      |
| SQL Server               | 2022 ou ultérieure   | https://www.microsoft.com/sql-server         |
| Redis                    | 7.x                  | https://redis.io                             |

---

## 🚀 Installation & configuration locale

### 1. Cloner le dépôt

```bash
git clone https://github.com/Kouumss/PulseERP.git
cd PulseERP
```

### 2. Configurer les secrets (hors Docker)

Les fichiers `appsettings*.json` contiennent des valeurs vides pour les secrets :

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "RedisSettings": {
    "Configuration": "redis:6379",
    "InstanceName": "PulseERP:"
  },
  "Jwt": {
    "SecretKey": ""
  },
  "Email": {
    "Password": ""
  }
}
```

En environnement de développement, utilisez les User Secrets pour sécuriser la chaîne de connexion, la clé JWT, le mot de passe SMTP, etc. :

# 1. Cloner le dépôt
```bash
git clone https://github.com/your-org/PulseERP.git
cd PulseERP
```

# 2. Configurer les secrets utilisateurs
#   dotnet user-secrets init --project PulseERP.API
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=.;Initial Catalog=PulseERP;User ID=sa;Password=Your_password123"
dotnet user-secrets set "RedisSettings:Configuration" "localhost:6379"
dotnet user-secrets set "RedisSettings:InstanceName" "PulseERP:"
dotnet user-secrets set "JwtSettings:SecretKey" "votre-clé-secrète-Ici"
dotnet user-secrets set "EmailSettings:Password" "votre-mot-de-passe-smtp"
dotnet user-secrets set "EmailSettings:FromEmail" "votre-email@domaine.com"
dotnet user-secrets set "AppSettings:FrontendUrl" "https://localhost:4200"
dotnet user-secrets set "AppSettings:FrontendClearUrl" "https://localhost:4200"
```
# 3. Restaurer & lancer
```bash
dotnet restore
dotnet build --configuration Release
dotnet run --project PulseERP.API
```


## 🔧 Compilation & tests locaux

```bash
# Restauration + build + tests
$ dotnet restore
$ dotnet build -c Release
$ dotnet test
```

### Couverture de code

```bash
$ dotnet test --collect:"XPlat Code Coverage"
$ reportgenerator -reports:**/coverage.cobertura.xml -targetdir:CoverageReport
```

Ouvre `CoverageReport/index.htm` dans ton navigateur.

---

## 🚀 Lancer l’API

```bash
$ cd PulseERP.API
$ dotnet run --launch-profile https
# Swagger : https://localhost:5001/swagger
```

## 🌀 CI GitHub Actions

Fichier : `.github/workflows/ci.yml`

```yaml
name: build-test
on:
  push:
    branches: [ main ]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 9.0.x
      - run: dotnet restore
      - run: dotnet build -c Release --no-restore
      - run: dotnet test --no-restore --collect:"XPlat Code Coverage"
      - uses: actions/upload-artifact@v4
        with:
          name: coverage
          path: '**/coverage.cobertura.xml'
```

Un push sur **main** ⇒ build, tests, rapport de couverture.

---

## 🤝 Contribuer

1. Fork : `git clone`
2. `git checkout -b feature/ma-feature`
3. Code + tests : `git commit -m "feat: …"`
4. `git push origin feature/ma-feature`
5. Ouvre une **Pull Request** (la CI se déclenchera)

---

## 📄 Licence

Ce projet est sous licence MIT.
Voir LICENSE.md pour plus de détails.


---

> Made with ❤️ & ☕ by the PulseERP maintainers.

![build](https://github.com/Kouumss/PulseERP/actions/workflows/ci.yml/badge.svg)
