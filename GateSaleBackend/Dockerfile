FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["GateSale.sln", "./"]
COPY ["GateSale.API/GateSale.API.csproj", "GateSale.API/"]
COPY ["GateSale.Core/GateSale.Core.csproj", "GateSale.Core/"]
COPY ["GateSale.Infrastructure/GateSale.Infrastructure.csproj", "GateSale.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "GateSale.API/GateSale.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/GateSale.API"
RUN dotnet build "GateSale.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GateSale.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "GateSale.API.dll"]
