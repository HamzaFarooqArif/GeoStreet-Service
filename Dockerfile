# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY . .
RUN dotnet restore "./GeoStreet-Service/GeoStreet-Service.csproj" --disable-parallel
RUN dotnet publish "./GeoStreet-Service/GeoStreet-Service.csproj" -c release -o /app --no-restore

# Serve Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./

EXPOSE 8080
EXPOSE 443

ENTRYPOINT ["dotnet", "GeoStreet-Service.dll"]