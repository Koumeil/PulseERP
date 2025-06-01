from pathlib import Path

readme_content = """![build](https://github.com/Kouumss/PulseERP/actions/workflows/ci.yml/badge.svg)

# PulseERP

> **PulseERP** est un back-end ERP modulaire, développé en **C# 13 / .NET 9**, destiné aux petites et moyennes entreprises.  
> Construit selon les principes de la **Clean Architecture** et du **Domain-Driven Design**, il se compose de couches Domain, Application, Abstractions, Infrastructure et API, couvertes par une suite de tests (xUnit + FluentAssertions) et une pipeline CI GitHub Actions qui compile et vérifie chaque commit.

---

## 🏗️ Structure du dépôt

```txt
PulseERP/
├─ .github/workflows/ci.yml         # Pipeline CI/CD (build + tests + couverture)
├─ docker-compose.yml               # Orchestration : Redis, SQL Server, API
├─ PulseERP.sln                     # Solution globale
│
├─ PulseERP.Domain/                 # Domaine métier (entités, VOs, erreurs)
│   ├─ Entities/                    # Product, User, Brand…
│   ├─ ValueObjects/                # Money, EmailAddress, Role…
│   └─ Errors/                      # DomainException, NotFoundException
│
├─ PulseERP.Abstractions/           # Ports & DTOs transverses
│   ├─ Common/                      # Pagination, Filters…
│   └─ Security/                    # Auth DTO, Token, interfaces (IEmailSender…)
│
├─ PulseERP.Application/            # Cas d’utilisation, services, mapping AutoMapper
│   ├─ Products/                    # Commands, Models, Services
│   ├─ Customers/                   # Commands, Models, Services
│   ├─ Mapping/                     # Profils AutoMapper
│   └─ DependencyInjection.cs       # Enregistrement dans IServiceCollection
│
├─ PulseERP.Infrastructure/         # Implémentations techniques
│   ├─ Data/                        # DbContext EF Core + Migrations
│   ├─ Repositories/                # Repos EF Core (avec cache Redis)
│   ├─ Providers/                   # DateTimeProvider, TokenHasher…
│   └─ Smtp/                        # EmailSender + templates MIME
│
├─ PulseERP.API/                    # ASP.NET Core 9 – endpoints REST
│   ├─ Contracts/                   # DTOs spécifiques à l’API (ApiResponse, etc.)
│   ├─ appsettings*.json            # Configuration (valeurs vides pour les secrets)
│   ├─ Program.cs                   # Construction du WebApplication
│   └─ Dockerfile                   # Dockerfile de l’API
│
├─ PulseERP.Tests/                  # xUnit + FluentAssertions (tests unitaires + intégration)
└─ README.md                        # Document de présentation (ci-dessous)
