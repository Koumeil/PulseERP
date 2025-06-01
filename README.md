# PulseERP

> **PulseERP** est un back-end ERP modulaire, développé en **C# 13 / .NET 9**, destiné aux petites et moyennes entreprises.
> Construit selon les principes de la **Clean Architecture** et du **Domain-Driven Design**, il se compose de couches Domain, Application, Abstractions, Infrastructure et API, couvertes par une suite de tests (xUnit + FluentAssertions) et une pipeline CI GitHub Actions qui compile et vérifie chaque commit.

---

## 🏡‍💻 Structure du dépôt

````txt
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
│        └─ Smtp/                        # EmailSender + templates MIME
│
├─ PulseERP.API/                    # ASP.NET Core 9 – endpoints REST
│   ├─ Contracts/                   # DTOs spécifiques à l’API (ApiResponse, etc.)
│   ├─ appsettings*.json            # Configuration (valeurs vides pour les secrets)
│   ├─ Program.cs                   # Construction du WebApplication
│   └─ Dockerfile                   # Dockerfile de l’API
│
├─ PulseERP.Tests/                  # xUnit + FluentAssertions (tests unitaires + intégration)
└─ README.md                        # Document de présentation (ci-dessous)

---

## ⚙️ Prérequis

| Outil    | Version           |
|----------|-------------------|
| .NET SDK | 9.0.300           |
| EF Core Tools | 9.0.*        |
| Docker & Docker Compose | ≥ 24.0 |

---

## 🔨 Installation & configuration locale

1. Cloner le dépôt

```bash
 git clone https://github.com/Kouumss/PulseERP.git
 cd PulseERP
````

2. Configurer les secrets (hors Docker)

Les fichiers `appsettings*.json` contiennent des clés vides pour les secrets :

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "RedisSettings": {
    "Configuration": "redis:6379",
    "InstanceName": "PulseERP:"
  }
}
```

En développement, utilisez les User Secrets pour stocker la chaîne de connexion locale, la clé JWT, le mot de passe SMTP, etc. :

```bash
 cd PulseERP.API
 dotnet user-secrets init
 dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=PulseERP;Trusted_Connection=True;"
 dotnet user-secrets set "Jwt:SecretKey"        "V3ryLongR@ndomKey..."
 dotnet user-secrets set "Email:Password"        "VotreMotDePasseSMTP"
```

3. Compiler et exécuter sans Docker

```bash
 dotnet restore
 dotnet build -c Release
 dotnet test
 cd PulseERP.API
 dotnet run --launch-profile https
```

L’API sera accessible sur `https://localhost:5001/swagger`.

### Variables d’environnement (hors Docker)

Si vous ne souhaitez pas User Secrets en local, vous pouvez définir explicitement :

```bash
 export ConnectionStrings__DefaultConnection="Server=.;Database=PulseERP;Trusted_Connection=True;"
 export Jwt__SecretKey="V3ryLongR@ndomKey..."
 export Email__Password="VotreMotDePasseSMTP"
 export RedisSettings__Configuration="localhost:6379"
 export RedisSettings__InstanceName="PulseERP:"
```

---

## 📦 Exécution via Docker Compose

Le fichier `docker-compose.yml` permet de lancer, en une seule commande :

* Redis (cache distribué)
* SQL Server (base de données)
* API PulseERP

### 1. Exemple de `docker-compose.yml`

```yaml
version: "3.9"

services:
  redis:
    image: redis:7-alpine
    container_name: pulseerp-redis
    ports:
      - "6379:6379"
    networks:
      - pulseerp-net

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: pulseerp-sql
    environment:
      SA_PASSWORD: "${SQL_SA_PASSWORD}"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
    volumes:
      - sql-data:/var/opt/mssql
    networks:
      - pulseerp-net

  pulseerp-api:
    build:
      context: .
      dockerfile: PulseERP.API/Dockerfile
    container_name: pulseerp-api
    depends_on:
      - redis
      - sqlserver
    ports:
      - "5000:80"
    environment:
      ConnectionStrings__DefaultConnection: "${SQL_SA_CONNECTION_STRING}"
      RedisSettings__Configuration:        "redis:6379"
      RedisSettings__InstanceName:         "PulseERP:"
    networks:
      - pulseerp-net

volumes:
  sql-data:

networks:
  pulseerp-net:
    driver: bridge
```

* **redis** : instance Redis (cache).
* **sqlserver** : SQL Server 2022, mot de passe SA injecté via la variable d’environnement `${SQL_SA_PASSWORD}`.
* **pulseerp-api** : construit depuis `PulseERP.API/Dockerfile`, dépend de Redis et SQL, injecte la chaîne de connexion via `${SQL_SA_CONNECTION_STRING}`, et les paramètres Redis.

> **Important** :
>
> * Ne commitez jamais les secrets en clair dans le YAML.
> * Utilisez un fichier `.env` (non versionné, ajouté à `.gitignore`) ou configurez directement ces variables dans votre shell ou votre plateforme CI/CD.

### 2. Démarrer les conteneurs

Dans un terminal, à la racine du projet :

```bash
 export SQL_SA_PASSWORD="Admin@123"
 export SQL_SA_CONNECTION_STRING="Server=sqlserver;Database=PulseERP;User Id=sa;Password=Admin@123;TrustServerCertificate=True;"
 docker-compose up -d
```

* Le conteneur **redis** écoute sur `localhost:6379`.
* Le conteneur **sqlserver** écoute sur `localhost:1433` (avec la DB `PulseERP`).
* L’API sera accessible sur `http://localhost:5000` (Swagger : `http://localhost:5000/swagger`).

### 3. Arrêter et nettoyer

```bash
 docker-compose down
```

---

## 🎉 Exécution de l’API (hors Docker)

```bash
 cd PulseERP.API
 dotnet run --launch-profile https
```

Swagger : `https://localhost:5001/swagger`

### Variables d’environnement essentielles :

| Nom                                    | Exemple                                              | Description                     |
| -------------------------------------- | ---------------------------------------------------- | ------------------------------- |
| ConnectionStrings\_\_DefaultConnection | Server=.;Database=PulseERP;Trusted\_Connection=True; | Chaîne de connexion SQL Server  |
| Jwt\_\_SecretKey                       | V3ryLongR\@ndomKey…                                  | Clé HMAC JWT 512 bits           |
| Email\_\_Password                      | VotreMotDePasseSMTP                                  | Mot de passe pour SMTP          |
| RedisSettings\_\_Configuration         | redis:6379                                           | Adresse Redis (hôte\:port)      |
| RedisSettings\_\_InstanceName          | PulseERP:                                            | Préfixe optionnel pour les clés |

---

## 📊 CI / GitHub Actions

Fichier : `.github/workflows/ci.yml`

```yaml
name: build-test
on:
  push:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest

    services:
      redis:
        image: redis:7-alpine
        ports:
          - 6379:6379

      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          SA_PASSWORD: ${{ secrets.SQL_SA_PASSWORD }}
          ACCEPT_EULA: "Y"
        ports:
          - 1433:1433

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET SDK
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 9.0.x

      - name: Restore & Build
        run: |
          dotnet restore PulseERP.sln
          dotnet build PulseERP.sln --no-restore --configuration Release

      - name: Publish API
        run: dotnet publish PulseERP.API/PulseERP.API.csproj -c Release -o ./publish

      - name: Build Docker image
        run: docker build -t pulseerp-api:ci ./PulseERP.API

      - name: Start services via Docker Compose
        env:
          SQL_SA_PASSWORD:        ${{ secrets.SQL_SA_PASSWORD }}
          SQL_SA_CONNECTION_STRING: ${{ secrets.SQL_SA_CONNECTION_STRING }}
        run: |
          cp docker-compose.yml .
          docker-compose up -d --build
          sleep 15   # Attendre que SQL et Redis soient prêts

      - name: Run tests
        run: dotnet test PulseERP.Tests/PulseERP.Tests.csproj --no-build --configuration Release

      - name: Teardown
        run: docker-compose down
```

**Secrets GitHub** :

* `SQL_SA_PASSWORD` : mot de passe SA
* `SQL_SA_CONNECTION_STRING` : chaîne de connexion complète ou mot de passe

À chaque push sur `main`, la CI :

1. Démarre Redis & SQL Server
2. Construit et publie l’API
3. Lance l’environnement via Docker Compose
4. Exécute les tests d’intégration
5. Arrête les conteneurs

---

## 🎪 Principes clés

* **Clean Architecture** : isoler le domaine, toutes les dépendances pointent vers `Domain`.
* **Domain-Driven Design** : Entities, Value Objects, Agrégats (Product, User, Brand), Ubiquitous Language, Domain Events.
* **Caching Redis** : implémenté dans `Infrastructure/Repositories` via `IDistributedCache` pour accélérer les lectures.
* **Validation & Résilience** : FluentValidation pour les commandes, Polly (retry, circuit-breaker) dans et hors infrastructure.
* **Logging & Tracing** : Serilog JSON + enrichers (UserId, CorrelationId) + configuration OpenTelemetry.

---

## 💪 Contribuer

1. Fork & clone :   `git clone https://github.com/Kouumss/PulseERP.git`   `cd PulseERP`
2. Créer une branche :   `git checkout -b feature/ma-feature`
3. Code + tests :   `git commit -m "feat: …"`
4. Pousser & ouvrir PR :   `git push origin feature/ma-feature`   Ouvrez une Pull Request (la CI se déclenchera automatiquement)

---

## 📖 Licence

Ce projet est protégé par une licence propriétaire. Toute utilisation, reproduction, modification ou commercialisation sans autorisation écrite préalable est strictement interdite.
Contact : [koumayl.messaoudi@gmail.com](mailto:koumayl.messaoudi@gmail.com)

*Made with ❤️ & ☕ by Team PulseERP.*
