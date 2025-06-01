# Étape 1 : build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copier la solution et tous les projets
COPY PulseERP.sln .
COPY PulseERP.*/*.csproj ./PulseERP.*

# Restaurer les dépendances à partir de la solution
RUN dotnet restore PulseERP.sln

# Copier le reste du code
COPY . .

# Build & publish du projet principal (PulseERP.API)
WORKDIR /src/PulseERP.API
RUN dotnet publish -c Release -o /app

# Étape 2 : image finale
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

EXPOSE 80
ENTRYPOINT ["dotnet", "PulseERP.API.dll"]
