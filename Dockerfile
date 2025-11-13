# Multi-stage Dockerfile for optimized image size
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Presentation/EasyBuy.WebAPI/EasyBuy.WebAPI.csproj", "Presentation/EasyBuy.WebAPI/"]
COPY ["Infrastructure/EasyBuy.Infrastructure/EasyBuy.Infrastructure.csproj", "Infrastructure/EasyBuy.Infrastructure/"]
COPY ["Infrastructure/EasyBuy.Persistence/EasyBuy.Persistence.csproj", "Infrastructure/EasyBuy.Persistence/"]
COPY ["Core/EasyBuy.Application/EasyBuy.Application.csproj", "Core/EasyBuy.Application/"]
COPY ["Core/EasyBuy.Domain/EasyBuy.Domain.csproj", "Core/EasyBuy.Domain/"]

RUN dotnet restore "Presentation/EasyBuy.WebAPI/EasyBuy.WebAPI.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Presentation/EasyBuy.WebAPI"
RUN dotnet build "EasyBuy.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EasyBuy.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "EasyBuy.WebAPI.dll"]
