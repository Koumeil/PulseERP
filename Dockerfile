# Étape 1 : build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copie des fichiers et restauration des dépendances
COPY *.sln .
COPY PulseERP.*/*.csproj ./PulseERP/
WORKDIR /app/PulseERP
RUN dotnet restore

# Copie du reste du code source
WORKDIR /app
COPY . .

# Compilation en mode Release
WORKDIR /app/PulseERP
RUN dotnet publish -c Release -o /out

# Étape 2 : image finale plus légère
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .

# Exposition du port par défaut pour une API ASP.NET
EXPOSE 80

# Point d'entrée
ENTRYPOINT ["dotnet", "PulseERP.Api.dll"]
