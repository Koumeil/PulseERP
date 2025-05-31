# PulseERP

>**PulseERP** est un back-end ERP modulaire conçu pour les petites et moyennes entreprises, développé en **C# 13 / .NET 9** 
Il applique rigoureusement les principes **Clean Architecture** & **Domain-Drive-Design**. : les couches Domain, Application, Abstractions, Infrastructure et API sont clairement séparées, testées (xUnit + FluentAssertions) et accompagnées d’une pipeline CI GitHub Actions qui construit, analyse et valide automatiquement chaque commit.

---

## 🏗️ Vue d’ensemble du dépôt

```txt
PulseERP/
├─ .github/workflows/ci.yml         # pipeline build+tests
├─ PulseERP.sln                     # solution globale
│
├─ PulseERP.Domain/                 # cœur métier (entités, VOs, erreurs)
│   ├─ Entities/                    # Product, User, Brand…
│   ├─ ValueObjects/                # Money, EmailAddress, Role, …
│   └─ Errors/                      # DomainException, NotFoundException
│
├─ PulseERP.Abstractions/           # Ports & DTO transverses
│   ├─ Security/                    # Auth DTO, Token, Interfaces (IEmailSender…)
│   └─ Common/                      # Pagination, Filters
│
├─ PulseERP.Application/            # Use‑cases, mapping, services
│   ├─ Products/                    # Commands, Models, Services
│   ├─ Customers/                   # Idem pour clients
│   ├─ Mapping/                     # Profils AutoMapper
│   └─ DependencyInjection.cs       # registres IServiceCollection
│
├─ PulseERP.Infrastructure/         # Implémentations techniques
│   ├─ Data/                        # DbContext EF Core + Migrations
│   ├─ Repositories/                # EF Core repos
│   ├─ Provider/                    # DateTimeProvider, TokenHasher…
│   └─ Smtp/                        # EmailSender + templates MIME
│
├─ PulseERP.API/                    # ASP.NET Core 9 – endpoints REST
│   ├─ Contracts/                   # DTO spécifiques API (ApiResponse, etc.)
│   ├─ appsettings*.json            # configuration
│   └─ Program.cs                   # WebApplication
│
└─ PulseERP.Tests/                  # xUnit + FluentAssertions (31 tests)
```

---

## ⚡ Prérequis

| Outil                  | Version |
| ---------------------- | ------- |
| **.NET SDK**           | 9.0.300 |
| **EF Core Tools**      | 9.0.\*  |
| **Docker** (optionnel) | ≥ 24.0  |

---

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

Variables d’environnement essentielles :

| Nom                          | Exemple                                                   | Description              |
| ---------------------------- | --------------------------------------------------------- | ------------------------ |
| `ConnectionStrings__Default` | `Server=.;Database=PulseERP;TrustServerCertificate=True;` | DB SQL Server/PostgreSQL |
| `Jwt__SecretKey`             | `V3ryLongR@ndomKey…`                                      | Clé HMAC JWT 512 bits    |
| `Email__Password`            | `***`                                                     | Mot de passe SMTP        |

---

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

## 🎯 Principes clés du projet

* **Clean Architecture** : toutes les dépendances pointent vers le domaine.
* **DDD** : Value Objects immuables (`Money`, `Role`, `EmailAddress`…), agrégats (`Product`, `User`).
* **Validation** : FluentValidation via pipeline `ValidationBehavior` dans Application.
* **Résilience** : Polly (retry, circuit‑breaker) bientôt ajouté dans Infrastructure.
* **Logging structuré** : Serilog JSON + enrichers (UserId, CorrelationId) + Export OpenTelemetry (à venir).

---

---

## 🤝 Contribuer

1. Fork : `git clone`
2. `git checkout -b feature/ma-feature`
3. Code + tests : `git commit -m "feat: …"`
4. `git push origin feature/ma-feature`
5. Ouvre une **Pull Request** (la CI se déclenchera)

---

## 📄 Licence

Distribué sous **MIT**. Voir `LICENSE`.

---

> Made with ❤️ & ☕ by the PulseERP maintainers.
