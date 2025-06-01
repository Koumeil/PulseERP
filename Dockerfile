# Étape 1 : build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copier la solution
COPY PulseERP.sln .

# Copier tous les dossiers de projets
COPY PulseERP.API ./PulseERP.API
COPY PulseERP.Domain ./PulseERP.Domain
COPY PulseERP.Application ./PulseERP.Application
COPY PulseERP.Infrastructure ./PulseERP.Infrastructure
COPY PulseERP.Tests ./PulseERP.Tests
COPY PulseERP.Abstractions ./PulseERP.Abstractions

# Restaurer les dépendances
RUN dotnet restore PulseERP.sln

# Publier l'API
WORKDIR /src/PulseERP.API
RUN dotnet publish -c Release -o /app

# Étape 2 : image finale
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

EXPOSE 80
ENTRYPOINT ["dotnet", "PulseERP.API.dll"]
